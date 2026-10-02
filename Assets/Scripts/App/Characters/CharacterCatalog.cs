using System;
using System.Collections.Generic;
using Supono.Animals;
using UnityEngine;

namespace Supono.App.Characters
{
    /// <summary>A character the player can start a level as. Its stats come from <see cref="definition"/>.</summary>
    [Serializable]
    public sealed class PlayableCharacter
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        [Min(0)] public int price;
        public AnimalDefinition definition;
        public GameObject playerPrefab;
    }

    /// <summary>Every selectable character, in display order. The first free one is the default.</summary>
    [CreateAssetMenu(menuName = "Supono/Character Catalog", fileName = "CharacterCatalog")]
    public sealed class CharacterCatalog : ScriptableObject
    {
        public List<PlayableCharacter> characters = new();

        public PlayableCharacter Default => characters.Find(c => c.price <= 0) ?? (characters.Count > 0 ? characters[0] : null);

        public PlayableCharacter Find(string id) => characters.Find(c => c.id == id);
    }
}
