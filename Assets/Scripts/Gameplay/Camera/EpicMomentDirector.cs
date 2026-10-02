using System;
using Cysharp.Threading.Tasks;
using Supono.Animals;
using Supono.Core;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Supono.Cameras
{
    /// <summary>
    /// Turns bites into moments: sometimes zooms in with slow motion when the player eats (more likely for
    /// big prey, always for the level goal), and always when the player gets eaten, holding on the predator.
    /// </summary>
    [RequireComponent(typeof(TopDownCameraFollow))]
    public sealed class EpicMomentDirector : MonoBehaviour
    {
        [Header("Player eats")]
        [SerializeField, Range(0f, 1f)] float baseChance = 0.15f;
        [SerializeField, Tooltip("Extra chance per unit of prey size relative to the player (prey/player).")]
        float chancePerRelativeSize = 0.7f;
        [SerializeField, Min(0f)] float cooldown = 6f;
        [SerializeField] float biteCloseUpDuration = 1.2f;
        [SerializeField, Range(0.05f, 1f)] float biteSlowMotion = 0.4f;
        [SerializeField] float biteSlowMotionDuration = 0.5f;

        [Header("Player eaten")]
        [SerializeField] float deathCloseUpDuration = 1.6f;
        [SerializeField, Range(0.05f, 1f)] float deathSlowMotion = 0.25f;
        [SerializeField] float deathSlowMotionDuration = 1f;

        [Header("Shake")]
        [SerializeField] float shakePerSize = 0.12f;

        TopDownCameraFollow follow;
        WorldRegistry registry;
        IGameplayModifiers modifiers;
        float nextMomentTime;

        [Inject]
        public void Construct(WorldRegistry registry, IGameplayModifiers modifiers)
        {
            this.registry = registry;
            this.modifiers = modifiers;
            registry.AnimalEaten += OnAnimalEaten;
        }

        void Awake() => follow = GetComponent<TopDownCameraFollow>();

        void OnDestroy()
        {
            if (registry != null) registry.AnimalEaten -= OnAnimalEaten;
        }

        void OnAnimalEaten(Animal victim, Animal eater)
        {
            if (eater == null || !modifiers.CinematicsEnabled) return;

            if (registry.IsPlayer(victim))
            {
                Moment(eater, deathCloseUpDuration, holdAtEnd: true, deathSlowMotion, deathSlowMotionDuration);
                return;
            }

            if (!registry.IsPlayer(eater)) return;

            float relativeSize = victim.Size / Mathf.Max(0.01f, eater.Size);
            follow.Shake(shakePerSize * relativeSize * eater.VisualSize, 0.25f);

            bool goal = registry.IsGoal(victim);
            float chance = baseChance + relativeSize * chancePerRelativeSize;
            if (!goal && (Time.unscaledTime < nextMomentTime || Random.value > chance)) return;

            nextMomentTime = Time.unscaledTime + cooldown;
            Moment(eater, biteCloseUpDuration, holdAtEnd: goal, biteSlowMotion, biteSlowMotionDuration);
        }

        void Moment(Animal subject, float duration, bool holdAtEnd, float slowMotion, float slowMotionDuration)
        {
            follow.PlayCloseUp(subject.transform, subject.VisualSize, duration, holdAtEnd);
            follow.Shake(shakePerSize * 2f * subject.VisualSize, 0.35f);
            SlowMotionAsync(slowMotion, slowMotionDuration).Forget();
        }

        async UniTaskVoid SlowMotionAsync(float scale, float duration)
        {
            Time.timeScale = scale;
            bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: true,
                cancellationToken: destroyCancellationToken).SuppressCancellationThrow();
            // Only undo our own slow motion; the level may have paused (0) in the meantime.
            if (!cancelled && Mathf.Approximately(Time.timeScale, scale)) Time.timeScale = 1f;
        }
    }
}
