using System;
using System.Collections.Generic;
using Supono.App.Saving;

namespace Supono.Debugging
{
    /// <summary>The debug menu's toggles, stored in the "debug" section of the save. All off = the default game.</summary>
    public sealed class DebugSettings
    {
        const string Section = "debug";

        [Serializable]
        sealed class Data
        {
            public List<string> enabled = new();
        }

        public readonly DebugToggle GodMode = new("god-mode", "God Mode", "Nothing can eat you.");
        public readonly DebugToggle InfiniteStamina = new("infinite-stamina", "Infinite Stamina", "Sprint forever.");
        public readonly DebugToggle CoinRush = new("coin-rush", "Coin Rush", "All coin gains in levels x5.");
        public readonly DebugToggle UnlockAllLevels = new("unlock-levels", "Unlock All Levels", "Every level is playable.");
        public readonly DebugToggle FreeCharacters = new("free-characters", "Free Characters", "Every character costs 0.");
        public readonly DebugToggle NoCinematics = new("no-cinematics", "No Cinematics", "Disable zoom-ins and slow motion.");
        public readonly DebugToggle DailyRewardEveryVisit = new("daily-every-visit", "Daily Reward Every Visit", "Show the daily reward on every main menu visit.");
        public readonly DebugToggle FailPurchases = new("fail-purchases", "Fail Purchases", "Every store purchase fails (test the error flow).");
        public readonly DebugToggle ForceTestVariant = new("ab-test-variant", "A/B: Test Variant", "Force the test variant of every A/B test.");
        public readonly DebugToggle OutlinesOff = new OutlinesOffToggle();
        public readonly DebugToggle Mute = new MuteToggle();

        readonly ISaveService saves;

        public IReadOnlyList<DebugToggle> All { get; }

        public DebugSettings(ISaveService saves)
        {
            this.saves = saves;
            All = new[]
            {
                GodMode, InfiniteStamina, CoinRush, UnlockAllLevels, FreeCharacters, NoCinematics,
                DailyRewardEveryVisit, FailPurchases, ForceTestVariant, OutlinesOff, Mute,
            };

            List<string> enabled = saves.Load<Data>(Section).enabled;
            foreach (DebugToggle toggle in All) toggle.Set(enabled != null && enabled.Contains(toggle.Id));
        }

        public void Set(DebugToggle toggle, bool on)
        {
            if (toggle.IsOn == on) return;
            toggle.Set(on);
            Save();
        }

        /// <summary>Turns everything off: back to the default game.</summary>
        public void ResetAll()
        {
            foreach (DebugToggle toggle in All) toggle.Set(false);
            Save();
        }

        void Save()
        {
            var data = new Data();
            foreach (DebugToggle toggle in All)
            {
                if (toggle.IsOn) data.enabled.Add(toggle.Id);
            }
            saves.Store(Section, data);
        }
    }
}
