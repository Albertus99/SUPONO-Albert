using UnityEngine;

namespace Supono.AI
{
    /// <summary>
    /// Designer-facing AI type. Each animal definition references one; it creates a fresh
    /// <see cref="IAiBehaviour"/> (with its own state) per animal. Add new AI by subclassing.
    /// </summary>
    public abstract class AiBehaviourAsset : ScriptableObject
    {
        public abstract IAiBehaviour CreateBehaviour();
    }
}
