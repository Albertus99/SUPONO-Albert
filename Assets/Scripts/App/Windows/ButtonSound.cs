using Supono.Audio;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Supono.App.Windows
{
    /// <summary>Plays a click when its button is pressed.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonSound : MonoBehaviour
    {
        [SerializeField] AudioClip clip;
        [SerializeField, Range(0f, 1f)] float volume = 0.6f;

        IAudioService audioService;

        [Inject]
        public void Construct(IAudioService audioService) => this.audioService = audioService;

        void Awake() => GetComponent<Button>().onClick.AddListener(() => audioService?.PlayUi(clip, volume));
    }
}
