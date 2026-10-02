using Supono.Audio;
using Supono.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Supono.App.Level
{
    /// <summary>
    /// Level scene scope; child of the project scope (settings, input, windows and progress come from the parent).
    /// Owns everything that lives and dies with the level.
    /// </summary>
    public sealed class LevelLifetimeScope : LifetimeScope
    {
        [SerializeField] Camera viewCamera;
        [SerializeField] LevelBounds bounds;
        [SerializeField] PlayerSpawnPoint spawnPoint;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(viewCamera);
            builder.RegisterInstance(bounds);
            builder.RegisterInstance(spawnPoint);
            builder.Register<WorldRegistry>(Lifetime.Singleton);
            builder.Register<PlayerSpawner>(Lifetime.Singleton);

            builder.RegisterEntryPoint<LevelCoins>().AsSelf();
            builder.RegisterEntryPoint<CoinBank>();
            builder.RegisterEntryPoint<LevelFlow>().AsSelf();
            builder.RegisterEntryPoint<HudPresenter>();
            builder.RegisterEntryPoint<GameplayAudio>();

            builder.RegisterBuildCallback(resolver =>
            {
                InjectScene(resolver);
                resolver.Resolve<PlayerSpawner>().Spawn();
            });
        }

        /// <summary>Injects every scene object (animals, camera, spawners, coins) once the container is built.</summary>
        void InjectScene(IObjectResolver resolver)
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                if (root != gameObject) resolver.InjectGameObject(root);
            }
        }
    }
}
