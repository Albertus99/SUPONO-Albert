using System;
using Supono.Animals;
using Supono.Core;
using UnityEngine;
using VContainer;

namespace Supono.Food
{
    /// <summary>A small bobbing snack any animal can eat. Grows the eater, refills stamina, or both.</summary>
    public sealed class FoodPickup : MonoBehaviour, IEdible
    {
        [SerializeField] FoodGroup group;
        [SerializeField] AudioClip eatenSound;
        [SerializeField, Min(0f)] float nutrition = 0.15f;
        [SerializeField, Range(0f, 1f), Tooltip("Stamina restored, as a fraction of max.")] float energy;
        [SerializeField, Tooltip("Compared against the eater's size; keep tiny so everyone can eat it.")]
        float size = 0.05f;
        [SerializeField] Transform visual;
        [SerializeField] float bobHeight = 0.1f;
        [SerializeField] float spinSpeed = 90f;

        WorldRegistry registry;
        Vector3 visualBase;
        float phase;

        public event Action<FoodPickup> Eaten;

        public float Size => size;
        public float Nutrition => nutrition;
        public float Energy => energy;
        public bool IsEdible => isActiveAndEnabled;
        public FoodGroup Group => group;
        public AudioClip EatenSound => eatenSound;
        public Vector3 Position => transform.position;

        [Inject]
        public void Construct(WorldRegistry registry)
        {
            this.registry = registry;
            if (isActiveAndEnabled) registry.Register(this);
        }

        void Awake()
        {
            if (visual != null) visualBase = visual.localPosition;
            phase = UnityEngine.Random.value * Mathf.PI * 2f;
        }

        void OnEnable() => registry?.Register(this);
        void OnDisable() => registry?.Unregister(this);

        void Update()
        {
            if (visual == null) return;
            visual.localPosition = visualBase + Vector3.up * (Mathf.Sin(Time.time * 2f + phase) * bobHeight);
            visual.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
        }

        public void BeEaten(Animal eater)
        {
            gameObject.SetActive(false);
            Eaten?.Invoke(this);
        }
    }
}
