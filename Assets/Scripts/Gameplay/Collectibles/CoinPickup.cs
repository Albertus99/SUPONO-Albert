using Supono.Animals;
using Supono.Core;
using UnityEngine;
using VContainer;

namespace Supono.Collectibles
{
    /// <summary>A spinning coin on the map. Only a <see cref="PlayerAvatar"/> can pick it up; other animals ignore it.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CoinPickup : MonoBehaviour
    {
        [SerializeField, Min(1)] int value = 1;
        [SerializeField] Transform visual;
        [SerializeField] float spinSpeed = 180f;
        [SerializeField] float bobHeight = 0.08f;

        LevelCoins coins;
        Vector3 visualBase;
        float phase;

        [Inject]
        public void Construct(LevelCoins coins) => this.coins = coins;

        void Awake()
        {
            if (visual != null) visualBase = visual.localPosition;
            phase = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            if (visual == null) return;
            visual.localPosition = visualBase + Vector3.up * (Mathf.Sin(Time.time * 3f + phase) * bobHeight);
            visual.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (coins == null || !other.TryGetComponent(out PlayerAvatar _)) return;
            if (!other.TryGetComponent(out Animal animal) || !animal.IsAlive) return;
            coins.Add(value);
            gameObject.SetActive(false);
        }
    }
}
