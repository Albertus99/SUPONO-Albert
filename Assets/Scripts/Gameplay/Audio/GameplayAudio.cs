using System;
using Supono.Animals;
using Supono.Core;
using VContainer.Unity;

namespace Supono.Audio
{
    /// <summary>
    /// Level sounds: when the player eats, the prey's own sound (its animal voice, or the food's munch) plus a bite;
    /// when the player is eaten, the predator's voice; a chime for every coin.
    /// </summary>
    public sealed class GameplayAudio : IStartable, IDisposable
    {
        readonly WorldRegistry registry;
        readonly LevelCoins coins;
        readonly IAudioService audio;
        readonly SoundLibrary sounds;
        Eater playerEater;

        public GameplayAudio(WorldRegistry registry, LevelCoins coins, IAudioService audio, SoundLibrary sounds)
        {
            this.registry = registry;
            this.coins = coins;
            this.audio = audio;
            this.sounds = sounds;
        }

        public void Start()
        {
            registry.AnimalEaten += OnAnimalEaten;
            coins.Changed += OnCoinsChanged;
            playerEater = registry.Player != null ? registry.Player.GetComponent<Eater>() : null;
            if (playerEater != null) playerEater.Ate += OnPlayerAte;
        }

        void OnPlayerAte(IEdible food)
        {
            audio.PlayAt(food.EatenSound, food.Position, sounds.voiceVolume);
            audio.PlayAt(sounds.bite, food.Position, sounds.biteVolume, 0.12f);
        }

        void OnAnimalEaten(Animal victim, Animal eater)
        {
            if (!registry.IsPlayer(victim) || eater == null) return;
            // The predator roars over the final bite.
            audio.PlayAt(eater.Definition.eatenSound, eater.Position, sounds.voiceVolume, 0f);
            audio.PlayAt(sounds.bite, victim.Position, sounds.biteVolume, 0f);
        }

        void OnCoinsChanged(int total) => audio.PlayUi(sounds.coin, sounds.coinVolume);

        public void Dispose()
        {
            registry.AnimalEaten -= OnAnimalEaten;
            coins.Changed -= OnCoinsChanged;
            if (playerEater != null) playerEater.Ate -= OnPlayerAte;
        }
    }
}
