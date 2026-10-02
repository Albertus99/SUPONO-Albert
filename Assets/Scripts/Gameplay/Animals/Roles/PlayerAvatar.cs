using Supono.Core;
using UnityEngine;
using VContainer;

namespace Supono.Animals
{
    /// <summary>
    /// Role: the animal the player controls. Added by composition (player prefabs only) and
    /// registers itself, so <see cref="Animal"/> carries no player-specific state.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animal))]
    public sealed class PlayerAvatar : MonoBehaviour
    {
        [Inject]
        public void Construct(WorldRegistry registry) => registry.SetPlayer(GetComponent<Animal>());
    }
}
