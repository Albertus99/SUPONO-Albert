using System;
using Supono.Core;
using UnityEngine;

namespace Supono.Animals
{
    /// <summary>Eats any edible the animal touches, as long as the animal is big enough.</summary>
    [RequireComponent(typeof(Animal), typeof(CharacterController))]
    public sealed class Eater : MonoBehaviour
    {
        [SerializeField, Tooltip("Mouth reach as a multiple of body radius.")]
        float reach = 1.2f;

        static readonly Collider[] Hits = new Collider[32];

        Animal self;
        CharacterController body;
        Stamina stamina;

        public event Action<IEdible> Ate;

        void Awake()
        {
            self = GetComponent<Animal>();
            body = GetComponent<CharacterController>();
            stamina = GetComponent<Stamina>();
        }

        void Update()
        {
            if (!self.IsAlive) return;

            Vector3 center = transform.TransformPoint(body.center);
            float halfSegment = Mathf.Max(0f, body.height * 0.5f - body.radius);
            Vector3 offset = Vector3.up * halfSegment;
            int count = Physics.OverlapCapsuleNonAlloc(center + offset, center - offset, body.radius * reach,
                Hits, Physics.AllLayers, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                if (!Hits[i].TryGetComponent(out IEdible prey) || !self.CanEat(prey)) continue;
                float nutrition = prey.Nutrition;
                float energy = prey.Energy;
                prey.BeEaten(self);
                self.Grow(nutrition);
                if (stamina != null && energy > 0f) stamina.Restore(energy);
                Ate?.Invoke(prey);
            }
        }
    }
}
