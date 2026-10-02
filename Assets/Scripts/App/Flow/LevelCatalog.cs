using System;
using System.Collections.Generic;
using UnityEngine;

namespace Supono.App.Flow
{
    /// <summary>Ordered list of playable levels and the main menu scene.</summary>
    [CreateAssetMenu(menuName = "Supono/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Level
        {
            public string displayName;
            public string scenePath;
            [Tooltip("Coins awarded on completion, on top of what was collected in the level.")]
            [Min(0)] public int completionReward = 50;
        }

        public string mainMenuScenePath;
        public List<Level> levels = new();

        public int IndexOf(string scenePath) => levels.FindIndex(level => level.scenePath == scenePath);
    }
}
