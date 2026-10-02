using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Supono.App.Extensions
{
    /// <summary>
    /// Optional module plugged into the project scope (debug tools, platform services...).
    /// Runs after the game's own registrations, so it may re-register an extension point
    /// (the last registration wins). The game never references a module; removing it from the
    /// <see cref="ProjectLifetimeScope"/> list leaves the plain game.
    /// </summary>
    public abstract class ProjectInstaller : ScriptableObject, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
