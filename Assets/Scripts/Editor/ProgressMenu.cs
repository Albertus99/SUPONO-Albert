using Supono.App;
using Supono.App.Characters;
using Supono.App.Flow;
using Supono.App.Progress;
using Supono.App.Saving;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Supono.Editor
{
    /// <summary>Testing shortcuts for the save: coins, unlocks, reset.</summary>
    static class ProgressMenu
    {
        [MenuItem("Tools/Supono/Progress/Add 1000 Coins")]
        static void AddCoins()
        {
            ProgressService progress = Progress();
            progress.AddCoins(1000);
            Debug.Log($"[Supono] Coins: {progress.Coins}");
        }

        [MenuItem("Tools/Supono/Progress/Unlock All Levels")]
        static void UnlockAll()
        {
            ProgressService progress = Progress();
            int levels = FindAsset<LevelCatalog>().levels.Count;
            for (int i = 0; i < levels - 1; i++) progress.CompleteLevel(i);
            Debug.Log("[Supono] All levels unlocked.");
        }

        [MenuItem("Tools/Supono/Progress/Reset Progress")]
        static void ResetProgress()
        {
            Progress().Reset();
            Debug.Log("[Supono] Progress reset.");
        }

        [MenuItem("Tools/Supono/Progress/Delete Save File (not while playing)")]
        static void DeleteSave()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[Supono] Stop play mode first: the running game would write its state back.");
                return;
            }
            new SaveService(new SaveFileStorage()).DeleteAll();
            Debug.Log("[Supono] Save file deleted.");
        }

        [MenuItem("Tools/Supono/Progress/Reveal Save File")]
        static void RevealSave() => EditorUtility.RevealInFinder(new SaveFileStorage().FilePath);

        /// <summary>The live service while playing (so open windows see the change), otherwise a fresh one over the same save.</summary>
        static ProgressService Progress()
        {
            if (Application.isPlaying)
            {
                var scope = LifetimeScope.Find<ProjectLifetimeScope>();
                if (scope != null && scope.Container != null) return scope.Container.Resolve<ProgressService>();
            }
            return new ProgressService(new SaveService(new SaveFileStorage()), FindAsset<CharacterCatalog>());
        }

        static T FindAsset<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            return guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
        }
    }
}
