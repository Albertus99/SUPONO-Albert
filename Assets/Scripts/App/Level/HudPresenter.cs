using Supono.Animals;
using Supono.App.Windows;
using Supono.Core;
using Supono.UI;
using VContainer.Unity;

namespace Supono.App.Level
{
    /// <summary>Pushes level state (goal, sizes, stamina, coins) into the HUD window while it is open.</summary>
    public sealed class HudPresenter : ITickable
    {
        readonly WorldRegistry registry;
        readonly IWindowManager windows;
        readonly GameSettings settings;
        readonly LevelCoins coins;
        Animal trackedPlayer;
        Stamina playerStamina;

        public HudPresenter(WorldRegistry registry, IWindowManager windows, GameSettings settings, LevelCoins coins)
        {
            this.registry = registry;
            this.windows = windows;
            this.settings = settings;
            this.coins = coins;
        }

        public void Tick()
        {
            if (!windows.TryGetOpen(out HudWindow hud)) return;

            Animal player = registry.Player;
            Animal goal = registry.Goal;
            if (player != trackedPlayer)
            {
                trackedPlayer = player;
                playerStamina = player != null ? player.GetComponent<Stamina>() : null;
            }

            bool playerAlive = player != null && player.IsAlive;
            bool goalAlive = goal != null && goal.IsAlive;
            hud.SetGoal(goal != null ? goal.DisplayName : null);
            hud.SetSizes(playerAlive ? player.Size : 0f, goalAlive ? goal.Size * settings.eatSizeRatio : 0f);
            hud.SetStamina(playerAlive && playerStamina != null ? playerStamina.Normalized : 0f,
                playerStamina != null && playerStamina.IsExhausted);
            hud.SetCoins(coins.Earned);
        }
    }
}
