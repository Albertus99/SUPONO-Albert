using Supono.Core;
using UnityEngine;
using VContainer;

namespace Supono.Animals
{
    /// <summary>
    /// Normalized (0..1) sprint stamina. Drains while sprinting; refills from energy food and, when the
    /// definition allows it, passively after a short pause. Running dry exhausts the animal until it
    /// recovers past a threshold.
    /// </summary>
    [RequireComponent(typeof(Animal))]
    public sealed class Stamina : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f), Tooltip("Stamina needed to sprint again after exhaustion.")]
        float recoverThreshold = 0.3f;
        [SerializeField, Min(0f), Tooltip("Seconds after sprinting before regeneration starts.")]
        float regenDelay = 0.75f;

        Animal animal;
        WorldRegistry registry;
        IGameplayModifiers modifiers = new DefaultGameplayModifiers();
        float regenBlockedUntil;

        public float Normalized { get; private set; } = 1f;
        public bool IsExhausted { get; private set; }
        public bool CanSprint => !IsExhausted && Normalized > 0f;

        [Inject]
        public void Construct(WorldRegistry registry, IGameplayModifiers modifiers)
        {
            this.registry = registry;
            this.modifiers = modifiers;
        }

        void Awake() => animal = GetComponent<Animal>();

        /// <summary>Called by the motor once per frame with whether it actually sprinted.</summary>
        public void Tick(bool sprinting, float deltaTime)
        {
            AnimalDefinition definition = animal.Definition;
            bool drains = !(modifiers.InfiniteStamina && registry != null && registry.IsPlayer(animal));
            if (sprinting && drains)
            {
                Normalized = Mathf.Max(0f, Normalized - deltaTime / Mathf.Max(0.01f, definition.sprintDuration));
                regenBlockedUntil = Time.time + regenDelay;
                if (Normalized <= 0f) IsExhausted = true;
            }
            else if (definition.passiveStaminaRecovery && Time.time >= regenBlockedUntil)
            {
                Normalized = Mathf.Min(1f, Normalized + deltaTime / Mathf.Max(0.01f, definition.staminaRecoveryDuration));
            }

            if (IsExhausted && Normalized >= recoverThreshold) IsExhausted = false;
        }

        public void Restore(float amount)
        {
            Normalized = Mathf.Clamp01(Normalized + amount);
            if (Normalized >= recoverThreshold) IsExhausted = false;
        }
    }
}
