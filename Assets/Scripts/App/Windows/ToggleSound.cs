using Supono.Audio;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Supono.App.Windows
{
    /// <summary>Plays a click when its toggle flips.</summary>
    [RequireComponent(typeof(Toggle))]
    public sealed class ToggleSound : MonoBehaviour
    {
        [SerializeField] AudioClip clip;
        [SerializeField, Range(0f, 1f)] float volume = 0.6f;

        IAudioService audioService;

        [Inject]
        public void Construct(IAudioService audioService) => this.audioService = audioService;

        void Awake() => GetComponent<Toggle>().onValueChanged.AddListener(_ => audioService?.PlayUi(clip, volume));
    }
}
