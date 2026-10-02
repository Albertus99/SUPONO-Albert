using System;
using System.Collections.Generic;
using System.Globalization;
using Supono.App.Experiments;
using Supono.App.Rules;

namespace Supono.App.Progress
{
    /// <summary>
    /// Login reward on a 7-day ladder, offered when the <see cref="IDailyRewardSchedule"/> says so (once per day).
    /// Claiming on consecutive days advances the streak; missing a day resets it to day 1. After day 7 the ladder starts over.
    /// The coin amounts come from the <see cref="IDailyRewardLadder"/> (A/B tested).
    /// </summary>
    public sealed class DailyRewardService
    {
        const string DateFormat = "yyyy-MM-dd";

        readonly ProgressService progress;
        readonly IClock clock;
        readonly IDailyRewardSchedule schedule;
        readonly IDailyRewardLadder ladder;

        public DailyRewardService(ProgressService progress, IClock clock, IDailyRewardSchedule schedule, IDailyRewardLadder ladder)
        {
            this.progress = progress;
            this.clock = clock;
            this.schedule = schedule;
            this.ladder = ladder;
        }

        public IReadOnlyList<int> Rewards => ladder.Rewards;

        /// <summary>The player's A/B group for the reward ladder.</summary>
        public string Group => ladder.Group;

        public bool IsAvailable => schedule.IsDue(LastClaim, clock.Today);

        /// <summary>The reward popup is on screen (A/B exposure).</summary>
        public void ReportShown() => ladder.ReportShown();

        /// <summary>1-based day on the ladder that claiming now would give.</summary>
        public int NextDay
        {
            get
            {
                DateTime? last = LastClaim;
                bool continues = last.HasValue && (last.Value == clock.Today.AddDays(-1) || last.Value == clock.Today);
                return continues ? progress.DailyStreak % Rewards.Count + 1 : 1;
            }
        }

        public int RewardFor(int day) => Rewards[day - 1];

        /// <summary>Grants today's reward and returns the coins granted.</summary>
        public int Claim()
        {
            int day = NextDay;
            int coins = RewardFor(day);
            progress.ClaimDailyReward(clock.Today.ToString(DateFormat, CultureInfo.InvariantCulture), day, coins);
            return coins;
        }

        DateTime? LastClaim =>
            DateTime.TryParseExact(progress.LastDailyClaim, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
                ? date
                : null;
    }
}
