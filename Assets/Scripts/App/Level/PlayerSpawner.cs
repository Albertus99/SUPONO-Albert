using Supono.App.Characters;
using Supono.App.Progress;
using Supono.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Supono.App.Level
{
    /// <summary>Spawns the character chosen in Character Selection at the level's spawn point.</summary>
    public sealed class PlayerSpawner
    {
        readonly IObjectResolver resolver;
        readonly ProgressService progress;
        readonly CharacterCatalog characters;
        readonly PlayerSpawnPoint spawnPoint;

        public PlayerSpawner(IObjectResolver resolver, ProgressService progress, CharacterCatalog characters, PlayerSpawnPoint spawnPoint)
        {
            this.resolver = resolver;
            this.progress = progress;
            this.characters = characters;
            this.spawnPoint = spawnPoint;
        }

        public GameObject Spawn()
        {
            PlayableCharacter character = characters.Find(progress.SelectedCharacterId) ?? characters.Default;
            Transform point = spawnPoint.transform;
            return resolver.Instantiate(character.playerPrefab, point.position, point.rotation);
        }
    }
}
