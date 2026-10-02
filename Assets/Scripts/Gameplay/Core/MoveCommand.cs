using UnityEngine;

namespace Supono.Core
{
    /// <summary>What a brain wants its body to do this frame.</summary>
    public readonly struct MoveCommand
    {
        public static readonly MoveCommand Stop = new MoveCommand(Vector3.zero, false);

        /// <summary>World-space planar direction. Magnitude (0..1) is throttle.</summary>
        public readonly Vector3 Direction;
        public readonly bool Sprint;

        public MoveCommand(Vector3 direction, bool sprint)
        {
            direction.y = 0f;
            Direction = Vector3.ClampMagnitude(direction, 1f);
            Sprint = sprint;
        }
    }
}
