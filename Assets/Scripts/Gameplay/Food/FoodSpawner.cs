using System;
using Cysharp.Threading.Tasks;
using Supono.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Supono.Food
{
    /// <summary>Keeps a fixed number of each food type scattered around the level, respawning eaten ones.</summary>
    public sealed class FoodSpawner : MonoBehaviour
    {
        [Serializable]
        struct Entry
        {
            public FoodPickup prefab;
            [Min(0)] public int count;
        }

        [SerializeField] Entry[] entries = Array.Empty<Entry>();
        [SerializeField, Min(0f)] float respawnDelay = 6f;
        [SerializeField, Min(0f)] float wallMargin = 2f;
        [SerializeField, Min(0f), Tooltip("Clearance from obstacles when picking a spawn point.")]
        float clearance = 0.5f;

        IObjectResolver resolver;
        LevelBounds bounds;

        [Inject]
        public void Construct(IObjectResolver resolver, LevelBounds bounds)
        {
            this.resolver = resolver;
            this.bounds = bounds;
        }

        void Start()
        {
            if (resolver == null) return;
            foreach (Entry entry in entries)
            {
                if (entry.prefab == null) continue;
                for (int i = 0; i < entry.count; i++)
                {
                    FoodPickup food = resolver.Instantiate(entry.prefab, NextSpawnPoint(), Quaternion.identity, transform);
                    food.Eaten += OnEaten;
                }
            }
        }

        Vector3 NextSpawnPoint() => bounds.RandomFreePoint(wallMargin, clearance);

        void OnEaten(FoodPickup food) => RespawnAsync(food).Forget();

        async UniTaskVoid RespawnAsync(FoodPickup food)
        {
            bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(respawnDelay),
                cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            if (cancelled || food == null) return;
            food.transform.position = NextSpawnPoint();
            food.gameObject.SetActive(true);
        }
    }
}
