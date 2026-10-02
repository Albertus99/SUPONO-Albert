using Supono.Animals;
using Supono.Core;
using UnityEngine;
using VContainer;

namespace Supono.AI
{
    /// <summary>
    /// AI implementation of <see cref="IAnimalBrain"/>. Decides at a fixed rate via a
    /// temperament strategy and steers toward the current intent every frame,
    /// sidestepping obstacles in the way.
    /// </summary>
    [RequireComponent(typeof(Animal), typeof(CharacterController))]
    public sealed class AiBrain : MonoBehaviour, IAnimalBrain
    {
        const float AvoidStepDegrees = 30f;
        const int AvoidSteps = 4;

        [SerializeField] float thinkInterval = 0.25f;
        [SerializeField, Tooltip("Distance from walls where fleeing starts steering back inside.")]
        float wallMargin = 3f;

        Animal self;
        CharacterController body;
        AiContext context;
        IAiBehaviour behaviour;
        AiIntent intent;
        float thinkTimer;
        float avoidSide = 1f;

        public AiIntent CurrentIntent => intent;

        [Inject]
        public void Construct(WorldRegistry registry, LevelBounds bounds)
        {
            self = GetComponent<Animal>();
            body = GetComponent<CharacterController>();
            context = new AiContext(self, registry, bounds, wallMargin);
            AiBehaviourAsset asset = self.Definition.behaviour;
            behaviour = asset != null ? asset.CreateBehaviour() : null;
            thinkTimer = Random.value * thinkInterval; // stagger so NPCs don't all think on the same frame
        }

        public MoveCommand Think(float deltaTime)
        {
            if (behaviour == null) return MoveCommand.Stop;

            thinkTimer -= deltaTime;
            if (thinkTimer <= 0f)
            {
                thinkTimer = thinkInterval;
                intent = behaviour.Decide(context);
            }

            if (intent == null) return MoveCommand.Stop;
            Vector3 direction = intent.Steer(context);
            return direction.sqrMagnitude < 0.0001f
                ? MoveCommand.Stop
                : new MoveCommand(AvoidObstacles(direction), intent.Sprint);
        }

        /// <summary>
        /// Probes ahead and fans out left/right until a clear heading is found.
        /// The probe starts above step height, so animals big enough to step over low obstacles ignore them.
        /// </summary>
        Vector3 AvoidObstacles(Vector3 direction)
        {
            if (IsClear(direction)) return direction;

            for (int step = 1; step <= AvoidSteps; step++)
            {
                Vector3 preferred = Quaternion.Euler(0f, avoidSide * step * AvoidStepDegrees, 0f) * direction;
                if (IsClear(preferred)) return preferred;

                Vector3 other = Quaternion.Euler(0f, -avoidSide * step * AvoidStepDegrees, 0f) * direction;
                if (IsClear(other))
                {
                    avoidSide = -avoidSide;
                    return other;
                }
            }

            return Quaternion.Euler(0f, avoidSide * 135f, 0f) * direction;
        }

        bool IsClear(Vector3 direction)
        {
            float radius = body.radius * 0.9f;
            Vector3 origin = transform.position + Vector3.up * (body.stepOffset + radius + 0.05f);
            float distance = body.radius + Mathf.Max(1f, self.WalkSpeed * 0.6f);
            return !Physics.SphereCast(origin, radius, direction, out _, distance,
                context.Bounds.ObstacleMask, QueryTriggerInteraction.Ignore);
        }
    }
}
