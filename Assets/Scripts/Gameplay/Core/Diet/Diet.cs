using UnityEngine;

namespace Supono.Core
{
    /// <summary>Strategy deciding what an eater will eat (size is checked separately).</summary>
    public abstract class Diet : ScriptableObject
    {
        public abstract bool Accepts(IEdible food);
    }
}
