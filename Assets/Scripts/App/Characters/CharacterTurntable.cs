using UnityEngine;

namespace Supono.App.Characters
{
    /// <summary>
    /// Spins one preview model: idles slowly, follows the finger while dragged and keeps the flick's
    /// momentum afterwards, easing back to the idle spin. Runs on unscaled time.
    /// </summary>
    public sealed class CharacterTurntable : MonoBehaviour
    {
        const float IdleSpeed = 30f;        // deg/s
        const float Friction = 2.5f;        // how fast a flick settles back to idle
        const float MaxFlickSpeed = 1080f;  // deg/s

        float velocity = IdleSpeed;
        float dragVelocity;
        bool dragging;

        /// <summary>Rotates by <paramref name="degrees"/> right now (a drag step).</summary>
        public void Drag(float degrees)
        {
            float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            dragging = true;
            transform.Rotate(0f, degrees, 0f, Space.World);
            dragVelocity = Mathf.Lerp(dragVelocity, degrees / dt, 0.5f);
        }

        /// <summary>Lets go: the model keeps spinning with the drag's speed.</summary>
        public void Release()
        {
            dragging = false;
            velocity = Mathf.Clamp(dragVelocity, -MaxFlickSpeed, MaxFlickSpeed);
            dragVelocity = 0f;
        }

        void Update()
        {
            if (dragging) return;
            float dt = Time.unscaledDeltaTime;
            float idle = velocity < 0f ? -IdleSpeed : IdleSpeed; // keep turning the way it was flicked
            velocity = Mathf.Lerp(velocity, idle, 1f - Mathf.Exp(-Friction * dt));
            transform.Rotate(0f, velocity * dt, 0f, Space.World);
        }
    }
}
