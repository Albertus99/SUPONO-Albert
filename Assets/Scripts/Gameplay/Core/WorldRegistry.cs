using System;
using System.Collections.Generic;
using Supono.Animals;
using Supono.Food;

namespace Supono.Core
{
    /// <summary>Tracks everything alive in the level. Used by AI senses and game rules.</summary>
    public sealed class WorldRegistry
    {
        readonly List<Animal> animals = new();
        readonly List<FoodPickup> food = new();

        public IReadOnlyList<Animal> Animals => animals;
        public IReadOnlyList<FoodPickup> Food => food;
        public Animal Player { get; private set; }
        public Animal Goal { get; private set; }

        /// <summary>(victim, eater)</summary>
        public event Action<Animal, Animal> AnimalEaten;

        public void Register(Animal animal)
        {
            if (!animals.Contains(animal)) animals.Add(animal);
        }

        /// <summary>Called by the <see cref="PlayerAvatar"/> role.</summary>
        public void SetPlayer(Animal animal) => Player = animal;

        /// <summary>Called by the <see cref="LevelGoal"/> role.</summary>
        public void SetGoal(Animal animal) => Goal = animal;

        public bool IsPlayer(Animal animal) => animal != null && animal == Player;
        public bool IsGoal(Animal animal) => animal != null && animal == Goal;

        public void Unregister(Animal animal) => animals.Remove(animal);

        public void Register(FoodPickup pickup)
        {
            if (!food.Contains(pickup)) food.Add(pickup);
        }

        public void Unregister(FoodPickup pickup) => food.Remove(pickup);

        public void ReportEaten(Animal victim, Animal eater)
        {
            animals.Remove(victim);
            AnimalEaten?.Invoke(victim, eater);
        }
    }
}
