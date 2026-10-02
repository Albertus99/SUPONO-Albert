using Supono.App.Characters;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Supono.App.MainMenu
{
    /// <summary>Main menu scene scope; child of the project scope.</summary>
    public sealed class MainMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] CharacterPreviewStage previewStagePrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(new CharacterPreviewFactory(previewStagePrefab));
            builder.RegisterEntryPoint<MainMenuFlow>();
        }
    }
}
