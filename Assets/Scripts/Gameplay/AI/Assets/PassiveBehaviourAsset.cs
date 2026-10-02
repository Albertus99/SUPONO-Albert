using UnityEngine;

namespace Supono.AI
{
    [CreateAssetMenu(menuName = "Supono/AI/Passive", fileName = "Passive")]
    public sealed class PassiveBehaviourAsset : AiBehaviourAsset
    {
        public override IAiBehaviour CreateBehaviour() => new PassiveBehaviour();
    }
}
