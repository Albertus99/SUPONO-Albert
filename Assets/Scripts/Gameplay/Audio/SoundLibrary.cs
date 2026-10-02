using UnityEngine;

namespace Supono.Audio
{
    /// <summary>Shared, non-animal sounds. Animal voices live on each AnimalDefinition.</summary>
    [CreateAssetMenu(menuName = "Supono/Sound Library", fileName = "SoundLibrary")]
    public sealed class SoundLibrary : ScriptableObject
    {
        [Header("Gameplay")]
        public AudioClip bite;
        public AudioClip munch;
        public AudioClip energy;
        public AudioClip coin;

        [Header("UI")]
        public AudioClip click;
        public AudioClip win;
        public AudioClip lose;
        public AudioClip reward;

        [Header("Music")]
        public AudioClip music;

        [Header("Mix")]
        [Range(0f, 1f)] public float musicVolume = 0.35f;
        [Range(0f, 1f)] public float voiceVolume = 0.9f;
        [Range(0f, 1f)] public float biteVolume = 0.7f;
        [Range(0f, 1f)] public float coinVolume = 0.45f;
        [Range(0f, 1f)] public float uiVolume = 0.6f;
    }
}
