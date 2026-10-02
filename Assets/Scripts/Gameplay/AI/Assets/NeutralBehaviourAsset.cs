using UnityEngine;

namespace Supono.AI
{
    [CreateAssetMenu(menuName = "Supono/AI/Neutral", fileName = "Neutral")]
    public sealed class NeutralBehaviourAsset : AiBehaviourAsset
    {
        [SerializeField, Range(0f, 1f), Tooltip("Fraction of the sense radius at which the player provokes it.")]
        float provokeRangeFraction = 0.35f;
        [SerializeField, Min(0f), Tooltip("Seconds it keeps charging once provoked.")]
        float provokedDuration = 4f;

        public override IAiBehaviour CreateBehaviour() => new NeutralBehaviour(provokeRangeFraction, provokedDuration);
    }
}
