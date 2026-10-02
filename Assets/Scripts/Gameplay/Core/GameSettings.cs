using UnityEngine;
using UnityEngine.Serialization;

namespace Supono.Core
{
    [CreateAssetMenu(menuName = "Supono/Game Settings", fileName = "GameSettings")]
    public sealed class GameSettings : ScriptableObject
    {
        [Header("Eating")]
        [Tooltip("The eater must be at least this many times bigger than its prey.")]
        [Min(1f)] public float eatSizeRatio = 1.1f;

        [Tooltip("Size gained = prey nutrition * growthFactor.")]
        [Min(0f)] public float growthFactor = 0.3f;

        [Tooltip("How fast the visual size eases toward the logical size.")]
        [Min(0.1f)] public float growthLerpSpeed = 5f;

        [Header("Economy")]
        [Tooltip("Coins for eating an animal = its size * this (at least 1).")]
        [Min(0f)] public float coinsPerSize = 10f;

        [Header("Level flow")]
        [Tooltip("Seconds of gameplay after the deciding bite before the result window opens.")]
        [FormerlySerializedAs("restartDelaySeconds")]
        [Min(0f)] public float resultDelaySeconds = 1.2f;

        [Header("Size indicators")]
        public Color edibleColor = new Color(0.3f, 0.9f, 0.4f);
        public Color dangerColor = new Color(0.95f, 0.25f, 0.2f);
        public Color neutralColor = new Color(0.95f, 0.85f, 0.3f);
    }
}
