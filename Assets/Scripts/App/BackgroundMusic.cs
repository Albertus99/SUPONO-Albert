using Supono.Audio;
using VContainer.Unity;

namespace Supono.App
{
    /// <summary>Starts the soundtrack at boot; it loops across scenes for the whole session.</summary>
    public sealed class BackgroundMusic : IStartable
    {
        readonly IAudioService audio;
        readonly SoundLibrary sounds;

        public BackgroundMusic(IAudioService audio, SoundLibrary sounds)
        {
            this.audio = audio;
            this.sounds = sounds;
        }

        public void Start() => audio.PlayMusic(sounds.music, sounds.musicVolume);
    }
}
