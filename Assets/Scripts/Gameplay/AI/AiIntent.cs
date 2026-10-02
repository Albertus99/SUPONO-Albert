using Supono.Animals;
using UnityEngine;

namespace Supono.AI
{
    /// <summary>
    /// A high-level decision that knows how to steer toward itself. Behaviours decide at a low rate;
    /// the brain asks the current intent for a direction every frame. Instances are reused, not allocated per decision.
    /// </summary>
    public abstract class AiIntent
    {
        public virtual bool Sprint => false;

        /// <summary>Desired planar direction this frame; zero means stand still.</summary>
        public abstract Vector3 Steer(AiContext context);

        /// <summary>True once the intent has nothing left to do (arrived, target gone...).</summary>
        public virtual bool IsComplete(AiContext context) => false;

        protected static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }

    public sealed class IdleIntent : AiIntent
    {
        public override Vector3 Steer(AiContext context) => Vector3.zero;
    }

    public sealed class MoveToIntent : AiIntent
    {
        public Vector3 Point { get; private set; }

        public MoveToIntent At(Vector3 point)
        {
            Point = point;
            return this;
        }

        public override Vector3 Steer(AiContext context) =>
            IsComplete(context) ? Vector3.zero : Flat(Point - context.Self.Position).normalized;

        public override bool IsComplete(AiContext context) =>
            context.DistanceTo(Point) <= context.Self.BodyRadius + 0.3f;
    }

    public sealed class ChaseIntent : AiIntent
    {
        public Animal Target { get; private set; }
        public override bool Sprint => true;

        public ChaseIntent Of(Animal target)
        {
            Target = target;
            return this;
        }

        public override Vector3 Steer(AiContext context) =>
            IsComplete(context) ? Vector3.zero : Flat(Target.Position - context.Self.Position).normalized;

        public override bool IsComplete(AiContext context) => Target == null || !Target.IsAlive;
    }

    public sealed class FleeIntent : AiIntent
    {
        public Animal Threat { get; private set; }
        public override bool Sprint => true;

        public FleeIntent From(Animal threat)
        {
            Threat = threat;
            return this;
        }

        public override Vector3 Steer(AiContext context)
        {
            if (IsComplete(context)) return Vector3.zero;
            Vector3 position = context.Self.Position;
            Vector3 away = Flat(position - Threat.Position).normalized;
            Vector3 direction = away + context.Bounds.Containment(position, context.WallMargin + context.Self.BodyRadius) * 2f;
            // Cornered: slide along the wall instead of freezing.
            if (direction.sqrMagnitude < 0.04f) direction = Vector3.Cross(Vector3.up, away);
            return direction.normalized;
        }

        public override bool IsComplete(AiContext context) => Threat == null || !Threat.IsAlive;
    }
}
