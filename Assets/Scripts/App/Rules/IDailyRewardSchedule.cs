using System;

namespace Supono.App.Rules
{
    /// <summary>Whether the daily reward can be claimed now.</summary>
    public interface IDailyRewardSchedule
    {
        bool IsDue(DateTime? lastClaim, DateTime today);
    }

    /// <summary>The normal game: once per calendar day.</summary>
    public sealed class OncePerDaySchedule : IDailyRewardSchedule
    {
        public bool IsDue(DateTime? lastClaim, DateTime today) => lastClaim != today;
    }
}
