using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.Animals;
using Supono.App.Analytics;
using Supono.App.Flow;
using Supono.App.Progress;
using Supono.Audio;
using Supono.App.Windows;
using Supono.Core;
using Supono.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Supono.App.Level
{
    public enum LevelState
    {
        Intro,
        Playing,
        Paused,
        Finished,
    }

    /// <summary>
    /// Level lifecycle: intro window → play (HUD) ⇄ pause → result window.
    /// Eat the goal animal to win; get eaten (or let something else eat the goal) to lose.
    /// Coins collected in the level are banked either way; winning adds the completion reward and unlocks the next level.
    /// The world is frozen with timeScale outside of Playing.
    /// All async steps run on the scope's lifetime token: when the level unloads they stop with an
    /// OperationCanceledException, which UniTaskVoid swallows.
    /// </summary>
    public sealed class LevelFlow : IStartable, IDisposable
    {
        readonly WorldRegistry registry;
        readonly IWindowManager windows;
        readonly GameFlow gameFlow;
        readonly GameSettings settings;
        readonly LevelCoins coins;
        readonly ProgressService progress;
        readonly IAudioService audio;
        readonly SoundLibrary sounds;
        readonly IAnalytics analytics;
        readonly CancellationTokenSource lifetime = new();
        HudWindow hud;
        float playStartedAt;

        public LevelState State { get; private set; } = LevelState.Intro;

        public LevelFlow(WorldRegistry registry, IWindowManager windows, GameFlow gameFlow, GameSettings settings,
            LevelCoins coins, ProgressService progress, IAudioService audio, SoundLibrary sounds, IAnalytics analytics)
        {
            this.analytics = analytics;
            this.audio = audio;
            this.sounds = sounds;
            this.registry = registry;
            this.windows = windows;
            this.gameFlow = gameFlow;
            this.settings = settings;
            this.coins = coins;
            this.progress = progress;
        }

        public void Start()
        {
            registry.AnimalEaten += OnAnimalEaten;
            windows.BackUnhandled += OnBack;
            RunIntroAsync(lifetime.Token).Forget();
        }

        async UniTaskVoid RunIntroAsync(CancellationToken cancellation)
        {
            SetWorldPaused(true);
            State = LevelState.Intro;

            string title = gameFlow.CurrentLevel?.displayName ?? SceneManager.GetActiveScene().name;
            string goal = registry.Goal != null ? registry.Goal.DisplayName : "biggest animal";
            string body = $"Eat smaller animals to grow.\nEat the <b>{goal}</b> to win!\n\n" +
                          "<size=26>WASD move   ·   Shift sprint   ·   Esc pause\n" +
                          "Food grows you. Stamina only refills from blue-ringed food. Grab the coins!</size>";

            LevelIntroWindow intro = await windows.OpenAsync<LevelIntroWindow>(w => w.Setup(title, body), cancellation);
            LevelIntroChoice choice = await intro.WaitForChoiceAsync(cancellation);
            await windows.CloseAsync(intro, cancellation);

            if (choice == LevelIntroChoice.MainMenu)
            {
                LeaveLevel(gameFlow.GoToMainMenuAsync());
                return;
            }

            // Subscribe before it shows so a pause click during the fade-in isn't lost.
            await windows.OpenAsync<HudWindow>(w =>
            {
                hud = w;
                w.PauseClicked += OnPauseClicked;
            }, cancellation);
            State = LevelState.Playing;
            SetWorldPaused(false);
            playStartedAt = Time.realtimeSinceStartup;
            analytics.Log(new LevelStartEvent(LevelNumber, LevelName, progress.SelectedCharacterId));
        }

        int LevelNumber => gameFlow.CurrentLevelIndex + 1;
        string LevelName => gameFlow.CurrentLevel?.displayName ?? SceneManager.GetActiveScene().name;

        void OnBack()
        {
            if (State == LevelState.Playing) PauseAsync(lifetime.Token).Forget();
        }

        void OnPauseClicked() => OnBack();

        async UniTaskVoid PauseAsync(CancellationToken cancellation)
        {
            State = LevelState.Paused;
            SetWorldPaused(true);

            PauseWindow pause = await windows.OpenAsync<PauseWindow>(cancellation: cancellation);
            PauseChoice choice = await pause.WaitForChoiceAsync(cancellation);
            await windows.CloseAsync(pause, cancellation);

            switch (choice)
            {
                case PauseChoice.Resume:
                    State = LevelState.Playing;
                    SetWorldPaused(false);
                    break;
                case PauseChoice.Restart:
                    LeaveLevel(gameFlow.RestartLevelAsync());
                    break;
                case PauseChoice.MainMenu:
                    LeaveLevel(gameFlow.GoToMainMenuAsync());
                    break;
            }
        }

        void OnAnimalEaten(Animal victim, Animal eater)
        {
            if (State != LevelState.Playing) return;

            if (registry.IsPlayer(victim))
                FinishAsync(false, "Eaten!", $"The {eater.DisplayName} got you.", lifetime.Token).Forget();
            else if (registry.IsGoal(victim) && registry.IsPlayer(eater))
                FinishAsync(true, "Victory!", $"You ate the {victim.DisplayName}!", lifetime.Token).Forget();
            else if (registry.IsGoal(victim))
                FinishAsync(false, "Too slow!", $"The {eater.DisplayName} ate the {victim.DisplayName} first.", lifetime.Token).Forget();
        }

        async UniTaskVoid FinishAsync(bool won, string title, string message, CancellationToken cancellation)
        {
            State = LevelState.Finished;

            // Bank first so leaving mid-delay (e.g. quitting) never loses the reward or the unlock.
            int reward = BankCoins(won);
            analytics.Log(new LevelEndEvent(LevelNumber, LevelName, won, reward, Time.realtimeSinceStartup - playStartedAt));
            audio.PlayUi(won ? sounds.win : sounds.lose, sounds.uiVolume);

            // Let the final bite play out before freezing the world.
            await UniTask.Delay(TimeSpan.FromSeconds(settings.resultDelaySeconds), cancellationToken: cancellation);
            SetWorldPaused(true);
            await windows.CloseAsync<HudWindow>(cancellation);

            bool hasNext = won && gameFlow.HasNextLevel;
            LevelResultWindow result = await windows.OpenAsync<LevelResultWindow>(
                w => w.Setup(won, title, message, hasNext, reward), cancellation);
            LevelResultChoice choice = await result.WaitForChoiceAsync(cancellation);

            switch (choice)
            {
                case LevelResultChoice.Retry:
                    LeaveLevel(gameFlow.RestartLevelAsync());
                    break;
                case LevelResultChoice.NextLevel:
                    LeaveLevel(gameFlow.StartNextLevelAsync());
                    break;
                case LevelResultChoice.MainMenu:
                    LeaveLevel(gameFlow.GoToMainMenuAsync());
                    break;
            }
        }

        /// <summary>
        /// Scene transitions are owned by the project scope and must not be cancelled by this (soon disposed) scope,
        /// so they are fired and forgotten rather than awaited with the level's token.
        /// </summary>
        static void LeaveLevel(UniTask transition) => transition.Forget();

        /// <summary>
        /// Collected coins are already saved as they're earned (<see cref="CoinBank"/>), win or lose.
        /// A win adds the completion reward and unlocks the next level. Returns the level's total for the result screen.
        /// </summary>
        int BankCoins(bool won)
        {
            int total = coins.Earned;
            int levelIndex = gameFlow.CurrentLevelIndex;
            if (won && levelIndex >= 0)
            {
                int reward = gameFlow.CurrentLevel.completionReward;
                progress.AddCoins(reward);
                progress.CompleteLevel(levelIndex);
                total += reward;
            }
            return total;
        }

        static void SetWorldPaused(bool paused) => Time.timeScale = paused ? 0f : 1f;

        public void Dispose()
        {
            registry.AnimalEaten -= OnAnimalEaten;
            windows.BackUnhandled -= OnBack;
            if (hud != null) hud.PauseClicked -= OnPauseClicked;
            lifetime.Cancel();
            lifetime.Dispose();
            SetWorldPaused(false);
        }
    }
}
