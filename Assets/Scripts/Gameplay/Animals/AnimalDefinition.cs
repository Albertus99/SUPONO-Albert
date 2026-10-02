using Supono.AI;
using Supono.Core;
using UnityEngine;

namespace Supono.Animals
{
    [CreateAssetMenu(menuName = "Supono/Animal Definition", fileName = "Animal")]
    public sealed class AnimalDefinition : ScriptableObject
    {
        public string displayName;
        public GameObject modelPrefab;
        [Tooltip("The animal's voice: played when the player eats it, and as its roar when it eats the player.")]
        public AudioClip eatenSound;

        [Header("Size")]
        [Tooltip("Starting size. 1 = the model's native scale (~1.5 units long).")]
        [Min(0.01f)] public float baseSize = 0.5f;
        [Tooltip("Capsule radius at size 1.")]
        [Min(0.01f)] public float bodyRadius = 0.6f;
        [Tooltip("Capsule height at size 1.")]
        [Min(0.01f)] public float bodyHeight = 1.6f;

        [Header("Diet")]
        [Tooltip("What this animal counts as when something eats it.")]
        public FoodGroup foodGroup;
        [Tooltip("What this animal is willing to eat.")]
        public Diet diet;

        [Header("Behaviour")]
        [Tooltip("AI used when this animal is not player-controlled.")]
        public AiBehaviourAsset behaviour;
        [Tooltip("World radius it notices others in (grows with sqrt of size).")]
        [Min(0f)] public float senseRadius = 8f;

        [Header("Movement")]
        [Tooltip("Walk speed at size 1. Actual speed scales with sqrt(size).")]
        [Min(0f)] public float walkSpeed = 3f;
        [Min(1f)] public float sprintMultiplier = 1.6f;

        [Header("Stamina")]
        [Tooltip("Seconds of continuous sprinting from full stamina.")]
        [Min(0.1f)] public float sprintDuration = 3f;
        [Tooltip("Off: stamina only refills by eating energy food (player animals).")]
        public bool passiveStaminaRecovery = true;
        [Tooltip("Seconds to passively refill stamina from empty, when passive recovery is on.")]
        [Min(0.1f)] public float staminaRecoveryDuration = 5f;
    }
}
