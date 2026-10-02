using System.Collections.Generic;
using UnityEngine;

namespace Supono.App.Characters
{
    /// <summary>Spawns the <see cref="CharacterPreviewStage"/> prefab out of sight, far below the world.</summary>
    public sealed class CharacterPreviewFactory
    {
        static readonly Vector3 StagePosition = new(0f, -500f, 0f);

        readonly CharacterPreviewStage prefab;

        public CharacterPreviewFactory(CharacterPreviewStage prefab) => this.prefab = prefab;

        /// <summary>A live stage showing <paramref name="characters"/>; destroy its GameObject when done.</summary>
        public CharacterPreviewStage Create(IReadOnlyList<PlayableCharacter> characters)
        {
            CharacterPreviewStage stage = Object.Instantiate(prefab, StagePosition, Quaternion.identity);
            stage.Show(characters);
            return stage;
        }
    }
}
