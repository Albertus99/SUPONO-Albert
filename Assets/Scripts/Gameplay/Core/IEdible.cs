using Supono.Animals;
using UnityEngine;

namespace Supono.Core
{
    /// <summary>Anything an animal can eat: other animals or food pickups.</summary>
    public interface IEdible
    {
        /// <summary>Size compared against the eater's size.</summary>
        float Size { get; }

        /// <summary>How much growth this gives when eaten.</summary>
        float Nutrition { get; }

        /// <summary>Stamina restored when eaten, as a fraction of max stamina.</summary>
        float Energy { get; }

        bool IsEdible { get; }
        /// <summary>What kind of food this is; the eater's <see cref="Diet"/> decides whether it accepts it.</summary>
        FoodGroup Group { get; }
        Vector3 Position { get; }

        /// <summary>Sound played when the player eats this (an animal's voice, a food's munch...).</summary>
        AudioClip EatenSound { get; }

        void BeEaten(Animal eater);
    }
}
