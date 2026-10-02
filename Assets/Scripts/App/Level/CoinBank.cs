using System;
using Supono.App.Progress;
using Supono.Core;
using VContainer.Unity;

namespace Supono.App.Level
{
    /// <summary>
    /// Saves coins to the player's progress the moment they are earned, so nothing is lost
    /// whether the level is won, lost, restarted, abandoned from the pause menu, or the game quits.
    /// </summary>
    public sealed class CoinBank : IStartable, IDisposable
    {
        readonly LevelCoins coins;
        readonly ProgressService progress;
        int banked;

        public CoinBank(LevelCoins coins, ProgressService progress)
        {
            this.coins = coins;
            this.progress = progress;
        }

        public void Start() => coins.Changed += OnCoinsChanged;

        void OnCoinsChanged(int earned)
        {
            progress.AddCoins(earned - banked);
            banked = earned;
        }

        public void Dispose() => coins.Changed -= OnCoinsChanged;
    }
}
