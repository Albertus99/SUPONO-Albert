using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Supono.App.Flow
{
    /// <summary>Top-level navigation between the main menu and levels. Lives in the project scope.</summary>
    public sealed class GameFlow
    {
        readonly SceneLoader loader;
        readonly LevelCatalog catalog;

        public GameFlow(SceneLoader loader, LevelCatalog catalog)
        {
            this.loader = loader;
            this.catalog = catalog;
        }

        public int CurrentLevelIndex => catalog.IndexOf(SceneManager.GetActiveScene().path);
        public LevelCatalog.Level CurrentLevel => CurrentLevelIndex >= 0 ? catalog.levels[CurrentLevelIndex] : null;
        public bool HasNextLevel => CurrentLevelIndex >= 0 && CurrentLevelIndex + 1 < catalog.levels.Count;
        public bool HasLevels => catalog.levels.Count > 0;

        public UniTask GoToMainMenuAsync() => loader.LoadAsync(catalog.mainMenuScenePath);

        public UniTask StartLevelAsync(int index) => loader.LoadAsync(catalog.levels[index].scenePath);

        public UniTask RestartLevelAsync() => loader.LoadAsync(SceneManager.GetActiveScene().path);

        public UniTask StartNextLevelAsync() =>
            HasNextLevel ? StartLevelAsync(CurrentLevelIndex + 1) : GoToMainMenuAsync();

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
