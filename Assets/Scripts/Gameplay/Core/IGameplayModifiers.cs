namespace Supono.Core
{
    /// <summary>
    /// Runtime rule overrides (driven by the debug menu). The default values describe the normal game;
    /// gameplay code only reads this interface and never knows about the debug UI.
    /// </summary>
    public interface IGameplayModifiers
    {
        bool PlayerInvulnerable { get; }
        bool InfiniteStamina { get; }
        float CoinMultiplier { get; }
        bool CinematicsEnabled { get; }
    }

    /// <summary>The normal game, used when no other modifiers are registered.</summary>
    public sealed class DefaultGameplayModifiers : IGameplayModifiers
    {
        public bool PlayerInvulnerable => false;
        public bool InfiniteStamina => false;
        public float CoinMultiplier => 1f;
        public bool CinematicsEnabled => true;
    }
}
