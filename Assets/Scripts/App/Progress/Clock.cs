using System;

namespace Supono.App.Progress
{
    /// <summary>Time source, injectable so date-based logic (daily rewards) can be tested.</summary>
    public interface IClock
    {
        DateTime Today { get; }
    }

    public sealed class SystemClock : IClock
    {
        public DateTime Today => DateTime.Now.Date;
    }
}
