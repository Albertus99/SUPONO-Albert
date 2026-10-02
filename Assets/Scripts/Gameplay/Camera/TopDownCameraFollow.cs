using Supono.Animals;
using Supono.Core;
using UnityEngine;
using VContainer;

namespace Supono.Cameras
{
    /// <summary>
    /// Isometric-style follow camera that pulls back as the player grows. Can blend into short
    /// cinematic close-ups and shake; both run on unscaled time so they keep moving in slow motion.
    /// </summary>
    public sealed class TopDownCameraFollow : MonoBehaviour
    {
        [SerializeField] float pitch = 55f;
        [SerializeField] float yaw = 45f;
        [SerializeField] float baseDistance = 3.5f;
        [SerializeField] float distancePerSize = 8f;
        [SerializeField] float followSharpness = 6f;
        [SerializeField] float zoomSharpness = 2f;

        [Header("Close-up")]
        [SerializeField] float closeUpPitch = 24f;
        [SerializeField] float closeUpDistancePerSize = 3.4f;
        [SerializeField] float closeUpMinDistance = 2.4f;
        [SerializeField, Tooltip("Degrees the camera orbits around the subject over the shot.")]
        float closeUpOrbit = 40f;
        [SerializeField, Range(0.05f, 0.5f), Tooltip("Fraction of the shot spent blending in (and out).")]
        float closeUpBlend = 0.22f;

        WorldRegistry registry;
        Vector3 focus;
        float distance;
        bool snapped;

        Transform closeUpTarget;
        float closeUpSize;
        float closeUpStart;
        float closeUpDuration;
        bool closeUpHold;

        float shakeAmplitude;
        float shakeStart;
        float shakeDuration;

        [Inject]
        public void Construct(WorldRegistry registry) => this.registry = registry;

        /// <summary>Cinematic shot of <paramref name="target"/>. With <paramref name="holdAtEnd"/> the camera stays on it.</summary>
        public void PlayCloseUp(Transform target, float targetSize, float duration, bool holdAtEnd = false)
        {
            closeUpTarget = target;
            closeUpSize = Mathf.Max(0.1f, targetSize);
            closeUpStart = Time.unscaledTime;
            closeUpDuration = Mathf.Max(0.1f, duration);
            closeUpHold = holdAtEnd;
        }

        public void Shake(float amplitude, float duration)
        {
            if (amplitude < shakeAmplitude * Remaining()) return; // don't cut a stronger shake short
            shakeAmplitude = amplitude;
            shakeStart = Time.unscaledTime;
            shakeDuration = Mathf.Max(0.01f, duration);
        }

        void LateUpdate()
        {
            Animal player = registry?.Player;
            if (player != null) Follow(player);
            if (!snapped) return;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 position = focus - rotation * Vector3.forward * distance;
            float focusDistance = distance;
            ApplyCloseUp(ref position, ref rotation, ref focusDistance);

            transform.SetPositionAndRotation(position + ShakeOffset(rotation), rotation);
            Shader.SetGlobalFloat(OutlineFocusId, focusDistance);
        }

        // The outline thins and fades relative to this, so zooming out never turns the scenery into ink noise.
        static readonly int OutlineFocusId = Shader.PropertyToID("_ToonOutlineFocusDistance");

        void OnDisable() => Shader.SetGlobalFloat(OutlineFocusId, 0f);

        void Follow(Animal player)
        {
            Vector3 targetFocus = player.Position + Vector3.up * (player.VisualSize * 0.5f);
            float targetDistance = baseDistance + player.VisualSize * distancePerSize;
            if (!snapped)
            {
                snapped = true;
                focus = targetFocus;
                distance = targetDistance;
                return;
            }

            float dt = Time.unscaledDeltaTime;
            focus = Vector3.Lerp(focus, targetFocus, 1f - Mathf.Exp(-followSharpness * dt));
            distance = Mathf.Lerp(distance, targetDistance, 1f - Mathf.Exp(-zoomSharpness * dt));
        }

        void ApplyCloseUp(ref Vector3 position, ref Quaternion rotation, ref float focusDistance)
        {
            if (closeUpTarget == null) return;

            float t = (Time.unscaledTime - closeUpStart) / closeUpDuration;
            if (t >= 1f && !closeUpHold)
            {
                closeUpTarget = null;
                return;
            }

            t = Mathf.Clamp01(t);
            float blendIn = Mathf.SmoothStep(0f, 1f, t / closeUpBlend);
            float blendOut = closeUpHold ? 1f : Mathf.SmoothStep(0f, 1f, (1f - t) / closeUpBlend);
            float weight = Mathf.Min(blendIn, blendOut);

            Vector3 subjectFocus = closeUpTarget.position + Vector3.up * (closeUpSize * 0.7f);
            float subjectDistance = Mathf.Max(closeUpMinDistance, closeUpSize * closeUpDistancePerSize);
            Quaternion subjectRotation = Quaternion.Euler(closeUpPitch, yaw + closeUpOrbit * (t - 0.5f), 0f);
            Vector3 subjectPosition = subjectFocus - subjectRotation * Vector3.forward * subjectDistance;

            position = Vector3.Lerp(position, subjectPosition, weight);
            rotation = Quaternion.Slerp(rotation, subjectRotation, weight);
            focusDistance = Mathf.Lerp(focusDistance, subjectDistance, weight);
        }

        Vector3 ShakeOffset(Quaternion rotation)
        {
            float strength = shakeAmplitude * Remaining();
            if (strength <= 0.0001f) return Vector3.zero;
            float time = Time.unscaledTime * 35f;
            var noise = new Vector2(Mathf.PerlinNoise(time, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, time) - 0.5f) * 2f;
            return rotation * new Vector3(noise.x, noise.y, 0f) * strength;
        }

        float Remaining() => Mathf.Clamp01(1f - (Time.unscaledTime - shakeStart) / shakeDuration);
    }
}
