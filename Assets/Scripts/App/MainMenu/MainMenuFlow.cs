using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.Animals;
using Supono.App.Analytics;
using Supono.App.Characters;
using Supono.App.Extensions;
using Supono.App.Flow;
using Supono.App.Progress;
using Supono.App.Rules;
using Supono.App.Shop;
using Supono.App.Windows;
using Supono.Audio;
using Supono.UI;
using UnityEngine;
using VContainer.Unity;

namespace Supono.App.MainMenu
{
    /// <summary>
    /// Main menu: daily reward on entry, then menu ⇄ level select / characters / coin shop / module screens
    /// until a level starts or the game quits.
    /// Runs on the scope's lifetime token: when the menu scene unloads every await throws OperationCanceledException
    /// and the flow stops (UniTaskVoid swallows it).
    /// </summary>
    public sealed class MainMenuFlow : IStartable, IDisposable
    {
        readonly IWindowManager windows;
        readonly GameFlow gameFlow;
        readonly LevelCatalog levels;
        readonly CharacterCatalog characters;
        readonly CharacterPreviewFactory previewFactory;
        readonly ProgressService progress;
        readonly DailyRewardService dailyRewards;
        readonly ILevelAccess levelAccess;
        readonly ICharacterPricing pricing;
        readonly CoinShop coinShop;
        readonly IReadOnlyList<IMainMenuExtension> extensions;
        readonly IAnalytics analytics;
        readonly IAudioService audio;
        readonly SoundLibrary sounds;
        readonly CancellationTokenSource lifetime = new();

        public MainMenuFlow(IWindowManager windows, GameFlow gameFlow, LevelCatalog levels, CharacterCatalog characters,
            CharacterPreviewFactory previewFactory, ProgressService progress, DailyRewardService dailyRewards, ILevelAccess levelAccess, ICharacterPricing pricing,
            CoinShop coinShop, IReadOnlyList<IMainMenuExtension> extensions, IAnalytics analytics, IAudioService audio, SoundLibrary sounds)
        {
            this.windows = windows;
            this.gameFlow = gameFlow;
            this.levels = levels;
            this.characters = characters;
            this.previewFactory = previewFactory;
            this.progress = progress;
            this.dailyRewards = dailyRewards;
            this.levelAccess = levelAccess;
            this.pricing = pricing;
            this.coinShop = coinShop;
            this.extensions = extensions;
            this.analytics = analytics;
            this.audio = audio;
            this.sounds = sounds;
        }

        public void Start() => RunAsync(lifetime.Token).Forget();

        async UniTaskVoid RunAsync(CancellationToken cancellation)
        {
            Time.timeScale = 1f;
            var extensionButtons = new List<MenuExtensionButton>();
            foreach (IMainMenuExtension extension in extensions) extensionButtons.Add(new MenuExtensionButton(extension.Label, extension.Icon));

            bool offerDailyReward = true;
            while (true)
            {
                MainMenuWindow menu = await windows.OpenAsync<MainMenuWindow>(
                    w => w.Setup(gameFlow.HasLevels, progress.Coins, extensionButtons), cancellation);
                if (offerDailyReward && dailyRewards.IsAvailable) await RunDailyRewardAsync(menu, cancellation);
                offerDailyReward = false;

                MainMenuChoice choice = await menu.WaitForChoiceAsync(cancellation);
                int extensionIndex = menu.ChosenExtension;
                await windows.CloseAsync(menu, cancellation);

                switch (choice)
                {
                    case MainMenuChoice.Play:
                        int level = await ChooseLevelAsync(cancellation);
                        if (level == LevelSelectWindow.Back) continue;
                        // Owned by the project scope; must outlive this menu scope, so not tied to its token.
                        gameFlow.StartLevelAsync(level).Forget();
                        return;
                    case MainMenuChoice.Characters:
                        await RunCharacterSelectAsync(cancellation);
                        continue;
                    case MainMenuChoice.Shop:
                        await RunCoinShopAsync(cancellation);
                        continue;
                    case MainMenuChoice.Extension:
                        await extensions[extensionIndex].RunAsync(cancellation);
                        continue;
                    case MainMenuChoice.Quit:
                        gameFlow.Quit();
                        return;
                }
            }
        }

        // ---------------------------------------------------------------- daily reward

        async UniTask RunDailyRewardAsync(MainMenuWindow menu, CancellationToken cancellation)
        {
            int coinsBefore = progress.Coins;
            int day = dailyRewards.NextDay;
            DailyRewardWindow window = await windows.OpenAsync<DailyRewardWindow>(
                w => w.Setup(day, DailyRewardService.Rewards), cancellation);
            await window.WaitForChoiceAsync(cancellation);

            int granted = dailyRewards.Claim();
            analytics.Log(new DailyRewardClaimEvent(day, granted));
            audio.PlayUi(sounds.reward, sounds.uiVolume);
            await window.PlayClaimAsync(menu.CoinTarget, () => audio.PlayUi(sounds.coin, sounds.coinVolume * 0.5f), cancellation);
            await windows.CloseAsync(window, cancellation);
            await menu.CountCoinsAsync(coinsBefore, progress.Coins, cancellation);
        }

        // ---------------------------------------------------------------- level select

        async UniTask<int> ChooseLevelAsync(CancellationToken cancellation)
        {
            var models = new List<LevelButtonModel>();
            for (int i = 0; i < levels.levels.Count; i++)
                models.Add(new LevelButtonModel(levels.levels[i].displayName, levelAccess.IsUnlocked(i), progress.IsLevelCompleted(i)));

            LevelSelectWindow window = await windows.OpenAsync<LevelSelectWindow>(w => w.Setup(models), cancellation);
            int index = await window.WaitForChoiceAsync(cancellation);
            await windows.CloseAsync(window, cancellation);
            return index;
        }

        // ---------------------------------------------------------------- character selection

        async UniTask RunCharacterSelectAsync(CancellationToken cancellation)
        {
            CharacterPreviewStage stage = previewFactory.Create(characters.characters);
            var tiles = new List<Rect>();
            for (int i = 0; i < characters.characters.Count; i++) tiles.Add(stage.TileRect(i));

            // Subscribe in the setup callback (before the window shows) so input during the fade-in isn't lost.
            CharacterSelectWindow window = null;
            try
            {
                window = await windows.OpenAsync<CharacterSelectWindow>(w =>
                {
                    window = w;
                    w.Setup(stage.Texture, tiles);
                    BindCharacters(w);
                    w.CardClicked += OnCardClicked;
                    w.CardSpun += OnCardSpun;
                    w.CardSpinReleased += OnCardSpinReleased;
                }, cancellation);
                await window.WaitForChoiceAsync(cancellation);
                await windows.CloseAsync(window, cancellation);
            }
            finally
            {
                if (window != null)
                {
                    window.CardClicked -= OnCardClicked;
                    window.CardSpun -= OnCardSpun;
                    window.CardSpinReleased -= OnCardSpinReleased;
                }
                UnityEngine.Object.Destroy(stage.gameObject); // after the fade-out, so the previews stay visible while closing
            }

            void OnCardSpun(int index, float degrees) => stage.Turntable(index).Drag(degrees);
            void OnCardSpinReleased(int index) => stage.Turntable(index).Release();

            void OnCardClicked(int index)
            {
                PlayableCharacter character = characters.characters[index];
                if (progress.Owns(character.id))
                {
                    progress.Select(character.id);
                    analytics.Log(new CharacterSelectEvent(character.id));
                }
                else
                {
                    int price = pricing.PriceOf(character);
                    if (!progress.TryUnlock(character, price)) return;
                    audio.PlayUi(sounds.reward, sounds.uiVolume);
                    analytics.Log(new CharacterUnlockEvent(character.id, price));
                    analytics.Log(new CharacterSelectEvent(character.id));
                }
                BindCharacters(window);
            }
        }

        void BindCharacters(CharacterSelectWindow window)
        {
            var models = new List<CharacterCardModel>();
            foreach (PlayableCharacter character in characters.characters)
            {
                CharacterCardState state = character.id == progress.SelectedCharacterId ? CharacterCardState.Selected
                    : progress.Owns(character.id) ? CharacterCardState.Owned
                    : CharacterCardState.Locked;
                int price = pricing.PriceOf(character);
                models.Add(new CharacterCardModel(character.displayName, character.description, Stats(character.definition),
                    price, state, progress.Coins >= price));
            }
            window.Bind(progress.Coins, models);
        }

        static string Stats(AnimalDefinition definition) =>
            $"Size <b>{definition.baseSize:0.00}</b>   Speed <b>{definition.walkSpeed:0.0}</b>   Sprint <b>{definition.sprintDuration:0.#}s</b>";

        // ---------------------------------------------------------------- coin shop (IAP)

        async UniTask RunCoinShopAsync(CancellationToken cancellation)
        {
            CoinShopWindow window = null;
            bool purchasing = false;
            try
            {
                window = await windows.OpenAsync<CoinShopWindow>(w =>
                {
                    window = w;
                    BindCoinShop(w);
                    w.PackClicked += OnPackClicked;
                }, cancellation);

                if (!await EnsureStoreAsync(window, cancellation)) BindCoinShop(window, "The store is unavailable right now. Please try again later.");

                await window.WaitForChoiceAsync(cancellation);
                await windows.CloseAsync(window, cancellation);
            }
            finally
            {
                if (window != null) window.PackClicked -= OnPackClicked;
            }

            void OnPackClicked(int index)
            {
                if (purchasing) return;
                purchasing = true;
                BuyAsync(window, index, cancellation).ContinueWith(() => purchasing = false).Forget();
            }
        }

        async UniTask<bool> EnsureStoreAsync(CoinShopWindow window, CancellationToken cancellation)
        {
            window.SetBusy("Connecting to the store...");
            bool ready = await coinShop.InitializeAsync(cancellation);
            window.SetBusy(null);
            BindCoinShop(window); // localized prices are known now
            return ready;
        }

        async UniTask BuyAsync(CoinShopWindow window, int index, CancellationToken cancellation)
        {
            CoinPack pack = coinShop.Catalog.packs[index];
            int coinsBefore = progress.Coins;

            window.SetBusy("Waiting for the store...");
            PurchaseResult result;
            try
            {
                result = await coinShop.BuyAsync(pack, cancellation);
            }
            finally
            {
                window.SetBusy(null);
            }

            audio.PlayUi(result.Succeeded ? sounds.reward : sounds.lose, sounds.uiVolume);
            PurchaseResultWindow popup = await windows.OpenAsync<PurchaseResultWindow>(w =>
            {
                if (result.Succeeded) w.ShowSuccess(pack.displayName, pack.coins);
                else w.ShowFailure(result.Message);
            }, cancellation);
            await popup.WaitForChoiceAsync(cancellation);
            await windows.CloseAsync(popup, cancellation);

            if (result.Succeeded)
            {
                audio.PlayUi(sounds.coin, sounds.coinVolume);
                await window.CountCoinsAsync(coinsBefore, progress.Coins, cancellation);
            }
        }

        void BindCoinShop(CoinShopWindow window, string note = null)
        {
            var models = new List<CoinPackModel>();
            foreach (CoinPack pack in coinShop.Catalog.packs)
                models.Add(new CoinPackModel(pack.displayName, pack.coins, coinShop.PriceOf(pack), pack.badge, pack.icon));

            // In the editor Unity IAP runs its simulated Fake Store: say so, so nobody worries about real charges.
            note ??= Application.isEditor ? "Sandbox store: purchases are simulated and nothing is charged." : string.Empty;
            window.Bind(progress.Coins, models, note);
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }
    }
}
