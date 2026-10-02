using Supono.App.Progress;

namespace Supono.App.Rules
{
    /// <summary>Which levels can be played.</summary>
    public interface ILevelAccess
    {
        bool IsUnlocked(int levelIndex);
    }

    /// <summary>The normal game: a level unlocks when the previous one is completed.</summary>
    public sealed class ProgressLevelAccess : ILevelAccess
    {
        readonly ProgressService progress;

        public ProgressLevelAccess(ProgressService progress) => this.progress = progress;

        public bool IsUnlocked(int levelIndex) => progress.IsLevelUnlocked(levelIndex);
    }
}
