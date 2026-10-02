using UnityEngine;

namespace Supono.Core
{
    /// <summary>
    /// A kind of food (plant, meat, ...). An asset rather than an enum, so designers can add groups
    /// without code; <see cref="Diet"/>s decide which groups they accept.
    /// </summary>
    [CreateAssetMenu(menuName = "Supono/Diet/Food Group", fileName = "FoodGroup")]
    public sealed class FoodGroup : ScriptableObject
    {
    }
}
