using System.Collections.Generic;
using UnityEngine;

namespace Supono.Core
{
    /// <summary>Accepts any food belonging to one of the listed groups.</summary>
    [CreateAssetMenu(menuName = "Supono/Diet/Food Group Diet", fileName = "Diet")]
    public sealed class FoodGroupDiet : Diet
    {
        [SerializeField] List<FoodGroup> accepts = new();

        public override bool Accepts(IEdible food) => food.Group != null && accepts.Contains(food.Group);
    }
}
