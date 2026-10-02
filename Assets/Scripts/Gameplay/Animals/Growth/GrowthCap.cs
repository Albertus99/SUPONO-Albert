using UnityEngine;

namespace Supono.Animals
{
    /// <summary>Strategy that limits how big an animal may grow. No limit component = unlimited growth.</summary>
    public interface IGrowthLimit
    {
        float Clamp(Animal animal, float proposedSize);
    }

    /// <summary>Caps growth at a multiple of the starting size. Used by NPCs so the goal stays reachable.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animal))]
    public sealed class GrowthCap : MonoBehaviour, IGrowthLimit
    {
        [SerializeField, Min(1f), Tooltip("Max size as a multiple of the starting size.")]
        float maxMultiplier = 1.3f;

        public float Clamp(Animal animal, float proposedSize) =>
            Mathf.Min(proposedSize, animal.InitialSize * maxMultiplier);
    }
}
