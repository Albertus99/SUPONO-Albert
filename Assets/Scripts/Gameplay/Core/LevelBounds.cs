using UnityEngine;

namespace Supono.Core
{
    /// <summary>Rectangular walkable area of the level, centered on this transform, plus obstacle queries.</summary>
    public sealed class LevelBounds : MonoBehaviour
    {
        [SerializeField] Vector2 size = new Vector2(70f, 70f);
        [SerializeField, Tooltip("Layers that block movement (walls, crates, trees...).")]
        LayerMask obstacleMask;

        public Vector3 Center => transform.position;
        public Vector2 Size => size;
        public LayerMask ObstacleMask => obstacleMask;

        public Vector3 RandomPoint(float margin)
        {
            float hx = Mathf.Max(0f, size.x * 0.5f - margin);
            float hz = Mathf.Max(0f, size.y * 0.5f - margin);
            return Center + new Vector3(Random.Range(-hx, hx), 0f, Random.Range(-hz, hz));
        }

        /// <summary>Random point not overlapping an obstacle. Falls back to the last try.</summary>
        public Vector3 RandomFreePoint(float margin, float radius, int attempts = 12)
        {
            Vector3 point = RandomPoint(margin);
            for (int i = 1; i < attempts && !IsFree(point, radius); i++) point = RandomPoint(margin);
            return point;
        }

        /// <summary>True when a sphere of <paramref name="radius"/> resting on the ground at <paramref name="point"/> touches no obstacle.</summary>
        public bool IsFree(Vector3 point, float radius) =>
            !Physics.CheckSphere(point + Vector3.up * (radius + 0.1f), radius, obstacleMask, QueryTriggerInteraction.Ignore);

        public Vector3 ClampInside(Vector3 point, float margin)
        {
            float hx = Mathf.Max(0f, size.x * 0.5f - margin);
            float hz = Mathf.Max(0f, size.y * 0.5f - margin);
            Vector3 local = point - Center;
            local.x = Mathf.Clamp(local.x, -hx, hx);
            local.z = Mathf.Clamp(local.z, -hz, hz);
            return Center + local;
        }

        /// <summary>Steering push toward the interior, growing from 0 to 1 within <paramref name="margin"/> of a wall.</summary>
        public Vector3 Containment(Vector3 point, float margin)
        {
            if (margin <= 0f) return Vector3.zero;
            Vector3 local = point - Center;
            float hx = size.x * 0.5f - margin;
            float hz = size.y * 0.5f - margin;
            Vector3 push = Vector3.zero;
            if (local.x > hx) push.x = -(local.x - hx) / margin;
            else if (local.x < -hx) push.x = (-hx - local.x) / margin;
            if (local.z > hz) push.z = -(local.z - hz) / margin;
            else if (local.z < -hz) push.z = (-hz - local.z) / margin;
            return push;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Center, new Vector3(size.x, 0.1f, size.y));
        }
    }
}
