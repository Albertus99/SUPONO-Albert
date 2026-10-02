using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Supono.Audio
{
    public interface IAudioService
    {
        /// <summary>Non-positional sound (UI, jingles). Unaffected by pause or slow motion.</summary>
        void PlayUi(AudioClip clip, float volume = 1f);

        /// <summary>World sound at <paramref name="position"/>, slightly randomized and pitched down in slow motion.</summary>
        void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float pitchVariance = 0.06f);

        /// <summary>Loops <paramref name="clip"/> as background music, fading in (no-op if it's already playing).</summary>
        void PlayMusic(AudioClip clip, float volume);
    }

    /// <summary>Pooled audio sources living for the whole session (project scope).</summary>
    public sealed class AudioService : IAudioService, IDisposable
    {
        const int PoolSize = 16;

        readonly GameObject root;
        const float MusicFadeSeconds = 2.5f;

        readonly AudioSource ui;
        readonly AudioSource music;
        readonly AudioSource[] pool = new AudioSource[PoolSize];
        readonly CancellationTokenSource lifetime = new();
        int next;

        public AudioService()
        {
            root = new GameObject("Audio");
            Object.DontDestroyOnLoad(root);

            ui = root.AddComponent<AudioSource>();
            ui.playOnAwake = false;
            ui.ignoreListenerPause = true;
            ui.spatialBlend = 0f;

            music = root.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.ignoreListenerPause = true;
            music.spatialBlend = 0f;
            music.loop = true;
            music.priority = 0; // never stolen by a burst of sound effects

            for (int i = 0; i < PoolSize; i++)
            {
                var go = new GameObject($"Sfx_{i}");
                go.transform.SetParent(root.transform, false);
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0.5f; // the camera follows the player, so keep sounds mostly centered
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 6f;
                source.maxDistance = 70f;
                pool[i] = source;
            }
        }

        public void PlayUi(AudioClip clip, float volume = 1f)
        {
            if (clip != null) ui.PlayOneShot(clip, volume);
        }

        public void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float pitchVariance = 0.06f)
        {
            if (clip == null) return;
            AudioSource source = pool[next];
            next = (next + 1) % PoolSize;
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume;
            // Slow motion drags the pitch down with it, which sells the moment.
            source.pitch = (1f + Random.Range(-pitchVariance, pitchVariance)) * Mathf.Lerp(0.55f, 1f, Mathf.Clamp01(Time.timeScale));
            source.Play();
        }

        public void PlayMusic(AudioClip clip, float volume)
        {
            if (clip == null || (music.clip == clip && music.isPlaying)) return;
            music.clip = clip;
            music.volume = 0f;
            music.Play();
            FadeMusicAsync(volume, lifetime.Token).Forget();
        }

        async UniTaskVoid FadeMusicAsync(float target, CancellationToken cancellation)
        {
            float start = music.volume;
            for (float time = 0f; time < MusicFadeSeconds; time += Time.unscaledDeltaTime)
            {
                music.volume = Mathf.Lerp(start, target, time / MusicFadeSeconds);
                if (await UniTask.Yield(PlayerLoopTiming.Update, cancellation).SuppressCancellationThrow()) return;
            }
            music.volume = target;
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
            if (root != null) Object.Destroy(root);
        }
    }
}
