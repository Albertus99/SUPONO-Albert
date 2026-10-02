using UnityEngine;

namespace Supono.Core
{
    /// <summary>Where the level spawns the player's chosen starting animal.</summary>
    public sealed class PlayerSpawnPoint : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.5f);
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 0.5f + transform.forward);
        }
    }
}
