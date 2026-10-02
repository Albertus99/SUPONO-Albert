using Supono.Core;
using UnityEngine;
using VContainer;

namespace Supono.Animals
{
    /// <summary>
    /// Role: the animal the player must eat to finish the level. Added to one animal per level
    /// and registers itself, so any animal can be made the goal without touching its prefab.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animal))]
    public sealed class LevelGoal : MonoBehaviour
    {
        [Inject]
        public void Construct(WorldRegistry registry) => registry.SetGoal(GetComponent<Animal>());
    }
}
