using Supono.App.Extensions;
using Supono.App.Rules;
using Supono.App.Shop;
using Supono.Core;
using Supono.UI;
using UnityEngine;
using VContainer;

namespace Supono.Debugging
{
    /// <summary>
    /// Plugs the debug tools into the project scope: the toggles, their rule overrides (each wrapping the
    /// game's default), the debug window and its main menu button. Remove this asset from the
    /// ProjectLifetimeScope's installers (e.g. for release) and the game runs without any debug code.
    /// </summary>
    [CreateAssetMenu(menuName = "Supono/Debug Installer", fileName = "DebugInstaller")]
    public sealed class DebugInstaller : ProjectInstaller
    {
        [SerializeField] WindowCatalog windows;
        [SerializeField] Sprite menuIcon;

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<DebugSettings>(Lifetime.Singleton);
            builder.RegisterBuildCallback(resolver => resolver.Resolve<DebugSettings>()); // apply persisted toggles at boot

            builder.Register<DebugGameplayModifiers>(Lifetime.Singleton).As<IGameplayModifiers>();
            builder.Register<DebugLevelAccess>(Lifetime.Singleton).As<ILevelAccess>();
            builder.Register<DebugCharacterPricing>(Lifetime.Singleton).As<ICharacterPricing>();
            builder.Register<DebugDailyRewardSchedule>(Lifetime.Singleton).As<IDailyRewardSchedule>();
            builder.Register<DebugIapStore>(Lifetime.Singleton).As<IIapStore>();

            builder.RegisterInstance(new DebugWindows(windows)).As<IWindowSource>();
            builder.Register<DebugMenuExtension>(Lifetime.Singleton).As<IMainMenuExtension>().WithParameter(menuIcon);
        }
    }

    /// <summary>The debug module's windows, offered to the window manager next to the game's catalog.</summary>
    public sealed class DebugWindows : IWindowSource
    {
        readonly WindowCatalog catalog;

        public DebugWindows(WindowCatalog catalog) => this.catalog = catalog;

        public T Find<T>() where T : Window => catalog.Find<T>();
    }
}
