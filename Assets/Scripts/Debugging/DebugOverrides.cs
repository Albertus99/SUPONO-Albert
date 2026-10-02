using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.App.Characters;
using Supono.App.Rules;
using Supono.App.Shop;
using Supono.Core;

namespace Supono.Debugging
{
    // Each override wraps the game's own implementation and only deviates while its toggle is on,
    // so with every toggle off the game behaves exactly as without this module.

    public sealed class DebugGameplayModifiers : IGameplayModifiers
    {
        readonly DefaultGameplayModifiers game;
        readonly DebugSettings debug;

        public DebugGameplayModifiers(DefaultGameplayModifiers game, DebugSettings debug)
        {
            this.game = game;
            this.debug = debug;
        }

        public bool PlayerInvulnerable => debug.GodMode.IsOn || game.PlayerInvulnerable;
        public bool InfiniteStamina => debug.InfiniteStamina.IsOn || game.InfiniteStamina;
        public float CoinMultiplier => game.CoinMultiplier * (debug.CoinRush.IsOn ? 5f : 1f);
        public bool CinematicsEnabled => !debug.NoCinematics.IsOn && game.CinematicsEnabled;
    }

    public sealed class DebugLevelAccess : ILevelAccess
    {
        readonly ProgressLevelAccess game;
        readonly DebugSettings debug;

        public DebugLevelAccess(ProgressLevelAccess game, DebugSettings debug)
        {
            this.game = game;
            this.debug = debug;
        }

        public bool IsUnlocked(int levelIndex) => debug.UnlockAllLevels.IsOn || game.IsUnlocked(levelIndex);
    }

    public sealed class DebugCharacterPricing : ICharacterPricing
    {
        readonly CatalogPricing game;
        readonly DebugSettings debug;

        public DebugCharacterPricing(CatalogPricing game, DebugSettings debug)
        {
            this.game = game;
            this.debug = debug;
        }

        public int PriceOf(PlayableCharacter character) => debug.FreeCharacters.IsOn ? 0 : game.PriceOf(character);
    }

    public sealed class DebugDailyRewardSchedule : IDailyRewardSchedule
    {
        readonly OncePerDaySchedule game;
        readonly DebugSettings debug;

        public DebugDailyRewardSchedule(OncePerDaySchedule game, DebugSettings debug)
        {
            this.game = game;
            this.debug = debug;
        }

        public bool IsDue(DateTime? lastClaim, DateTime today) => debug.DailyRewardEveryVisit.IsOn || game.IsDue(lastClaim, today);
    }

    /// <summary>Lets QA see the purchase failure feedback on demand, without touching the store.</summary>
    public sealed class DebugIapStore : IIapStore
    {
        readonly UnityIapStore game;
        readonly DebugSettings debug;

        public DebugIapStore(UnityIapStore game, DebugSettings debug)
        {
            this.game = game;
            this.debug = debug;
        }

        public event Action<string> Purchased
        {
            add => game.Purchased += value;
            remove => game.Purchased -= value;
        }

        public bool IsReady => game.IsReady;

        public UniTask<bool> InitializeAsync(CancellationToken cancellation) => game.InitializeAsync(cancellation);

        public string PriceOf(string productId) => game.PriceOf(productId);

        public async UniTask<PurchaseResult> PurchaseAsync(string productId, CancellationToken cancellation)
        {
            if (!debug.FailPurchases.IsOn) return await game.PurchaseAsync(productId, cancellation);
            await UniTask.Delay(TimeSpan.FromSeconds(0.8f), ignoreTimeScale: true, cancellationToken: cancellation);
            return PurchaseResult.Failure(productId, "The payment was declined (simulated by the debug menu). You were not charged.");
        }
    }
}
