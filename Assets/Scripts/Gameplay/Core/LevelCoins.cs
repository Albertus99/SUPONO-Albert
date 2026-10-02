using System;
using Supono.Animals;
using UnityEngine;
using VContainer.Unity;

namespace Supono.Core
{
    /// <summary>Coins earned during the current level: map pickups plus a bounty for every animal the player eats.</summary>
    public sealed class LevelCoins : IStartable, IDisposable
    {
        readonly WorldRegistry registry;
        readonly GameSettings settings;
        readonly IGameplayModifiers modifiers;

        public int Earned { get; private set; }

        public event Action<int> Changed;

        public LevelCoins(WorldRegistry registry, GameSettings settings, IGameplayModifiers modifiers)
        {
            this.registry = registry;
            this.settings = settings;
            this.modifiers = modifiers;
        }

        public void Start() => registry.AnimalEaten += OnAnimalEaten;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Earned += Mathf.Max(1, Mathf.RoundToInt(amount * modifiers.CoinMultiplier));
            Changed?.Invoke(Earned);
        }

        void OnAnimalEaten(Animal victim, Animal eater)
        {
            if (registry.IsPlayer(eater))
                Add(Mathf.Max(1, Mathf.RoundToInt(victim.Size * settings.coinsPerSize)));
        }

        public void Dispose() => registry.AnimalEaten -= OnAnimalEaten;
    }
}
