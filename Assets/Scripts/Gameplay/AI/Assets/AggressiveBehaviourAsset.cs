using UnityEngine;

namespace Supono.AI
{
    [CreateAssetMenu(menuName = "Supono/AI/Aggressive", fileName = "Aggressive")]
    public sealed class AggressiveBehaviourAsset : AiBehaviourAsset
    {
        [SerializeField, Range(0.1f, 1f), Tooltip("< 1 makes the player look closer than other prey.")]
        float playerBias = 0.7f;
        [SerializeField, Min(0f)] float maxChaseDuration = 5f;
        [SerializeField, Min(0f)] float restDuration = 3f;

        public override IAiBehaviour CreateBehaviour() => new AggressiveBehaviour(playerBias, maxChaseDuration, restDuration);
    }
}
