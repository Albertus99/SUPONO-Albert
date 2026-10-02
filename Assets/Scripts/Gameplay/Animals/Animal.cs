using System;
using Cysharp.Threading.Tasks;
using Supono.Core;
using UnityEngine;
using VContainer;

namespace Supono.Animals
{
    /// <summary>
    /// Core animal entity: owns size, growth and being eaten.
    /// Movement, eating and decision-making live in sibling components; roles such as
    /// <see cref="PlayerAvatar"/> or <see cref="LevelGoal"/> and growth limits (<see cref="IGrowthLimit"/>)
    /// are composed onto it rather than flagged.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class Animal : MonoBehaviour, IEdible
    {
        [SerializeField] AnimalDefinition definition;
        [SerializeField] Transform visualRoot;
        [SerializeField, Tooltip("Per-instance starting size. 0 uses the definition's base size.")]
        float sizeOverride;
        [SerializeField] float deathDuration = 0.25f;

        CharacterController body;
        IGrowthLimit growthLimit;
        WorldRegistry registry;
        GameSettings settings;
        IGameplayModifiers modifiers = new DefaultGameplayModifiers();
        float targetSize;
        float visualSize;
        bool initialized;

        public event Action<Animal> Grew;

        public AnimalDefinition Definition => definition;
        public string DisplayName => definition.displayName;
        public bool IsAlive { get; private set; } = true;

        /// <summary>Logical size used for eat checks. The visual size eases toward it.</summary>
        public float Size => targetSize;
        public float VisualSize => visualSize;
        public float InitialSize => sizeOverride > 0f ? sizeOverride : definition.baseSize;
        public float BodyRadius => body.radius;

        public float WalkSpeed => definition.walkSpeed * Mathf.Sqrt(targetSize);
        public float SprintSpeed => WalkSpeed * definition.sprintMultiplier;
        public float SenseRadius => definition.senseRadius * Mathf.Max(1f, Mathf.Sqrt(targetSize));

        float IEdible.Nutrition => targetSize;
        float IEdible.Energy => 0f;
        bool IEdible.IsEdible => IsAlive;
        FoodGroup IEdible.Group => definition.foodGroup;
        AudioClip IEdible.EatenSound => definition.eatenSound;
        public Vector3 Position => transform.position;

        [Inject]
        public void Construct(WorldRegistry registry, GameSettings settings, IGameplayModifiers modifiers)
        {
            this.registry = registry;
            this.settings = settings;
            this.modifiers = modifiers;
            Initialize();
            registry.Register(this);
        }

        void Awake() => Initialize();

        void Initialize()
        {
            if (initialized) return;
            initialized = true;
            body = GetComponent<CharacterController>();
            growthLimit = GetComponent<IGrowthLimit>();
            targetSize = visualSize = InitialSize;
            ApplySize(visualSize);
        }

        void OnDestroy() => registry?.Unregister(this);

        void Update()
        {
            if (!IsAlive || settings == null || Mathf.Approximately(visualSize, targetSize)) return;
            visualSize = Mathf.Lerp(visualSize, targetSize, 1f - Mathf.Exp(-settings.growthLerpSpeed * Time.deltaTime));
            if (Mathf.Abs(visualSize - targetSize) < 0.001f) visualSize = targetSize;
            ApplySize(visualSize);
        }

        public bool CanEat(IEdible prey)
        {
            if (!IsAlive || prey == null || ReferenceEquals(prey, this) || !prey.IsEdible) return false;
            if (definition.diet == null || !definition.diet.Accepts(prey)) return false;
            if (modifiers.PlayerInvulnerable && registry != null && ReferenceEquals(prey, registry.Player)) return false;
            return targetSize >= prey.Size * settings.eatSizeRatio;
        }

        public void Grow(float nutrition)
        {
            float next = targetSize + nutrition * settings.growthFactor;
            if (growthLimit != null) next = growthLimit.Clamp(this, next);
            if (next <= targetSize) return;
            targetSize = next;
            Grew?.Invoke(this);
        }

        public void BeEaten(Animal eater)
        {
            if (!IsAlive) return;
            IsAlive = false;
            body.enabled = false;
            registry?.ReportEaten(this, eater);
            DieAsync(eater != null ? eater.transform : null).Forget();
        }

        async UniTaskVoid DieAsync(Transform eater)
        {
            var token = this.GetCancellationTokenOnDestroy();
            Vector3 startPosition = transform.position;
            float startSize = visualSize;
            for (float t = 0f; t < deathDuration; t += Time.deltaTime)
            {
                float k = t / deathDuration;
                if (eater != null) transform.position = Vector3.Lerp(startPosition, eater.position, k);
                ApplySize(Mathf.Lerp(startSize, 0.001f, k));
                if (await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow()) return;
            }
            Destroy(gameObject);
        }

        void ApplySize(float size)
        {
            if (visualRoot != null) visualRoot.localScale = Vector3.one * size;

            float radius = definition.bodyRadius * size;
            float height = Mathf.Max(definition.bodyHeight * size, radius * 2f);
            body.radius = radius;
            body.height = height;
            body.center = new Vector3(0f, height * 0.5f, 0f);
            body.skinWidth = Mathf.Max(0.002f, radius * 0.1f);
            body.stepOffset = height * 0.25f;
            body.minMoveDistance = 0f;
        }
    }
}
