using Supono.Animals;
using UnityEngine;

namespace Supono.AI
{
    /// <summary>Turns perception into an intent. Created per animal by an <see cref="AiBehaviourAsset"/>.</summary>
    public interface IAiBehaviour
    {
        AiIntent Decide(AiContext context);
    }

    /// <summary>Fleeing and wandering, shared by every behaviour. Owns reusable intent instances.</summary>
    public abstract class AiBehaviourBase : IAiBehaviour
    {
        const float WallMargin = 2f;
        const float GrazeChance = 0.6f;
        const float RestChance = 0.3f;

        readonly IdleIntent idle = new();
        readonly MoveToIntent wanderMove = new();
        readonly ChaseIntent chase = new();
        readonly FleeIntent flee = new();
        AiIntent wander;
        float nextWanderTime;

        public abstract AiIntent Decide(AiContext context);

        protected AiIntent Chase(Animal target) => chase.Of(target);

        protected bool TryFlee(AiContext context, out AiIntent intent)
        {
            Animal threat = context.FindNearestThreat();
            intent = threat != null ? flee.From(threat) : null;
            return threat != null;
        }

        /// <summary>Strolls to nearby points or food, with occasional rests.</summary>
        protected AiIntent Wander(AiContext context)
        {
            if (wander != null && context.Time < nextWanderTime && !wander.IsComplete(context)) return wander;

            nextWanderTime = context.Time + Random.Range(3f, 7f);
            var food = Random.value < GrazeChance ? context.FindNearestFood() : null;
            if (food != null) wander = wanderMove.At(food.Position);
            else if (Random.value < RestChance) wander = idle;
            else wander = wanderMove.At(PickWanderPoint(context));
            return wander;
        }

        static Vector3 PickWanderPoint(AiContext context)
        {
            Animal self = context.Self;
            float range = Mathf.Max(4f, self.SenseRadius);
            Vector3 point = self.Position;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * range;
                point = context.Bounds.ClampInside(self.Position + new Vector3(offset.x, 0f, offset.y), WallMargin + self.BodyRadius);
                if (context.Bounds.IsFree(point, self.BodyRadius)) break;
            }
            return point;
        }
    }

    /// <summary>Never attacks. Runs from anything that could eat it.</summary>
    public sealed class PassiveBehaviour : AiBehaviourBase
    {
        public override AiIntent Decide(AiContext context) =>
            TryFlee(context, out AiIntent flee) ? flee : Wander(context);
    }

    /// <summary>
    /// Ignores the player unless it is bigger than the player and the player
    /// comes too close, then charges for a while. Flees when outsized.
    /// </summary>
    public sealed class NeutralBehaviour : AiBehaviourBase
    {
        readonly float provokeRangeFraction;
        readonly float provokedDuration;
        float provokedUntil;

        public NeutralBehaviour(float provokeRangeFraction, float provokedDuration)
        {
            this.provokeRangeFraction = provokeRangeFraction;
            this.provokedDuration = provokedDuration;
        }

        public override AiIntent Decide(AiContext context)
        {
            if (TryFlee(context, out AiIntent flee)) return flee;

            Animal self = context.Self;
            Animal player = context.Registry.Player;
            if (player != null && player.IsAlive && self.CanEat(player))
            {
                float distance = context.DistanceTo(player.Position);
                float provokeRange = self.SenseRadius * provokeRangeFraction + self.BodyRadius + player.BodyRadius;
                if (distance < provokeRange) provokedUntil = context.Time + provokedDuration;
                if (context.Time < provokedUntil && distance < self.SenseRadius) return Chase(player);
            }

            return Wander(context);
        }
    }

    /// <summary>
    /// Hunts the nearest thing it can eat (prefers the player). Gives up after
    /// a long chase or when out of stamina, and rests, so it can be outrun. Flees when outsized.
    /// </summary>
    public sealed class AggressiveBehaviour : AiBehaviourBase
    {
        readonly float playerBias;
        readonly float maxChaseDuration;
        readonly float restDuration;
        Animal quarry;
        float chaseStarted;
        float restUntil;

        public AggressiveBehaviour(float playerBias, float maxChaseDuration, float restDuration)
        {
            this.playerBias = playerBias;
            this.maxChaseDuration = maxChaseDuration;
            this.restDuration = restDuration;
        }

        public override AiIntent Decide(AiContext context)
        {
            if (TryFlee(context, out AiIntent flee))
            {
                quarry = null;
                return flee;
            }

            if (context.Time < restUntil) return Wander(context);

            Animal prey = context.FindNearestPrey(playerBias);
            if (prey == null)
            {
                quarry = null;
                return Wander(context);
            }

            if (prey != quarry)
            {
                quarry = prey;
                chaseStarted = context.Time;
            }

            if (context.Time - chaseStarted > maxChaseDuration || context.IsExhausted)
            {
                quarry = null;
                restUntil = context.Time + restDuration;
                return Wander(context);
            }

            return Chase(prey);
        }
    }
}
