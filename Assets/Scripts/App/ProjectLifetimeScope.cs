using System.Collections.Generic;
using Supono.App.Analytics;
using Supono.App.Characters;
using Supono.App.Extensions;
using Supono.App.Flow;
using Supono.App.Progress;
using Supono.App.Rules;
using Supono.App.Saving;
using Supono.App.Shop;
using Supono.Audio;
using Supono.Core;
using Supono.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Supono.App
{
    /// <summary>
    /// Root scope (set in VContainerSettings). Created once before the first scene and kept alive:
    /// shared settings, input, UI/window system, saves, progress, store, analytics and scene navigation.
    /// Main menu and level scopes are its children. Optional modules (e.g. debug tools) plug in through
    /// <see cref="installers"/>; the game itself never references them.
    /// </summary>
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] GameSettings settings;
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] LevelCatalog levelCatalog;
        [SerializeField] WindowCatalog windowCatalog;
        [SerializeField] CharacterCatalog characterCatalog;
        [SerializeField] CoinPackCatalog coinPackCatalog;
        [SerializeField] SoundLibrary soundLibrary;
        [SerializeField] UIRoot uiRootPrefab;
        [SerializeField, Tooltip("Optional modules, installed after the game's own registrations.")]
        List<ProjectInstaller> installers = new();

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(settings);
            builder.RegisterInstance(inputActions);
            builder.RegisterInstance(levelCatalog);
            builder.RegisterInstance(windowCatalog);
            builder.RegisterInstance(characterCatalog);
            builder.RegisterInstance(coinPackCatalog);
            builder.RegisterInstance(soundLibrary);

            builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();
            builder.RegisterEntryPoint<BackgroundMusic>();

            builder.RegisterComponentInNewPrefab(uiRootPrefab, Lifetime.Singleton).DontDestroyOnLoad();
            builder.RegisterEntryPoint<WindowManager>().AsSelf();

            // One save file for everything.
            builder.Register<SaveFileStorage>(Lifetime.Singleton).As<ISaveStorage>();
            builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();

            builder.Register<ProgressService>(Lifetime.Singleton);
            builder.Register<SystemClock>(Lifetime.Singleton).As<IClock>();
            builder.Register<DailyRewardService>(Lifetime.Singleton);

            builder.Register<ConsoleAnalytics>(Lifetime.Singleton).As<IAnalytics>();
#if SUPONO_FIREBASE
            builder.Register<FirebaseAnalytics>(Lifetime.Singleton).As<IAnalytics>();
#endif

            builder.Register<UnityIapStore>(Lifetime.Singleton).AsSelf().As<IIapStore>();
            builder.RegisterEntryPoint<CoinShop>().AsSelf();

            // Extension points with the normal game's rules. Modules may re-register them (last one wins)
            // and wrap the defaults, which stay resolvable by their concrete type.
            builder.Register<DefaultGameplayModifiers>(Lifetime.Singleton).AsSelf().As<IGameplayModifiers>();
            builder.Register<ProgressLevelAccess>(Lifetime.Singleton).AsSelf().As<ILevelAccess>();
            builder.Register<CatalogPricing>(Lifetime.Singleton).AsSelf().As<ICharacterPricing>();
            builder.Register<OncePerDaySchedule>(Lifetime.Singleton).AsSelf().As<IDailyRewardSchedule>();

            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<GameFlow>(Lifetime.Singleton);

            foreach (ProjectInstaller installer in installers)
            {
                if (installer != null) installer.Install(builder);
            }
        }
    }
}
