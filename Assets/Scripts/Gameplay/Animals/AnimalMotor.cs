using Supono.Core;
using UnityEngine;

namespace Supono.Animals
{
    /// <summary>
    /// Moves the animal with a CharacterController according to its brain's commands.
    /// Shared by the player and every NPC.
    /// </summary>
    [RequireComponent(typeof(Animal), typeof(CharacterController))]
    public sealed class AnimalMotor : MonoBehaviour
    {
        [SerializeField, Tooltip("Degrees per second.")] float turnSpeed = 540f;
        [SerializeField, Tooltip("Velocity change per second, as a multiple of sprint speed.")] float acceleration = 8f;
        [SerializeField] float gravity = -30f;

        Animal animal;
        CharacterController body;
        IAnimalBrain brain;
        Stamina stamina;
        Vector3 planarVelocity;
        float verticalSpeed;

        public float PlanarSpeed => planarVelocity.magnitude;

        void Awake()
        {
            animal = GetComponent<Animal>();
            body = GetComponent<CharacterController>();
            brain = GetComponent<IAnimalBrain>();
            stamina = GetComponent<Stamina>();
        }

        /// <summary>Swap control at runtime, e.g. possess an NPC or hand the player to AI.</summary>
        public void SetBrain(IAnimalBrain value) => brain = value;

        void Update()
        {
            if (!animal.IsAlive || !body.enabled) return;

            float dt = Time.deltaTime;
            MoveCommand command = brain != null ? brain.Think(dt) : MoveCommand.Stop;

            bool sprinting = command.Sprint
                             && command.Direction.sqrMagnitude > 0.01f
                             && (stamina == null || stamina.CanSprint);
            if (stamina != null) stamina.Tick(sprinting, dt);

            float topSpeed = sprinting ? animal.SprintSpeed : animal.WalkSpeed;
            Vector3 desired = command.Direction * topSpeed;
            planarVelocity = Vector3.MoveTowards(planarVelocity, desired, animal.SprintSpeed * acceleration * dt);

            verticalSpeed = body.isGrounded ? -1f : verticalSpeed + gravity * dt;
            body.Move((planarVelocity + Vector3.up * verticalSpeed) * dt);

            // Use the resolved velocity so we don't keep "running" into walls.
            Vector3 resolved = body.velocity;
            planarVelocity = new Vector3(resolved.x, 0f, resolved.z);

            if (command.Direction.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(command.Direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * dt);
            }
        }
    }
}
