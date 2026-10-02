using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    /// <summary>7-day reward ladder. Resolves when the player claims; then plays the coin flight.</summary>
    public sealed class DailyRewardWindow : ChoiceWindow<bool>
    {
        [SerializeField] List<DailyRewardDayView> days = new();
        [SerializeField] Button claimButton;
        [SerializeField] Image flyingCoinTemplate;
        [SerializeField, Min(1)] int flyingCoins = 12;

        int today;

        protected override void Awake()
        {
            base.Awake();
            flyingCoinTemplate.gameObject.SetActive(false);
            claimButton.onClick.AddListener(() => Choose(true));
        }

        /// <param name="nextDay">1-based day being claimed today.</param>
        public void Setup(int nextDay, IReadOnlyList<int> rewards)
        {
            today = nextDay;
            claimButton.interactable = true;
            for (int i = 0; i < days.Count; i++)
            {
                int day = i + 1;
                days[i].Bind(day, rewards[i], claimed: day < nextDay, isToday: day == nextDay);
            }
        }

        /// <summary>Coins burst out of today's tile and fly into <paramref name="target"/>; <paramref name="onCoinArrived"/> fires per coin.</summary>
        public async UniTask PlayClaimAsync(RectTransform target, Action onCoinArrived, CancellationToken cancellation)
        {
            claimButton.interactable = false;
            DailyRewardDayView tile = days[today - 1];
            tile.MarkClaimed();

            Vector3 from = tile.transform.position;
            var flights = new List<UniTask>();
            for (int i = 0; i < flyingCoins; i++)
            {
                flights.Add(FlyAsync(from, target, i * 0.045f, onCoinArrived, cancellation));
            }
            await UniTask.WhenAll(flights);
        }

        async UniTask FlyAsync(Vector3 from, RectTransform target, float delay, Action onArrived, CancellationToken cancellation)
        {
            Image coin = Instantiate(flyingCoinTemplate, flyingCoinTemplate.transform.parent);
            coin.gameObject.SetActive(true);
            Transform t = coin.transform;
            t.position = from;
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true, cancellationToken: cancellation);

                // Burst outward, then arc into the counter.
                Vector3 burst = from + (Vector3)(UnityEngine.Random.insideUnitCircle * 140f);
                const float duration = 0.65f;
                for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
                {
                    float k = time / duration;
                    Vector3 a = Vector3.Lerp(from, burst, k);
                    Vector3 b = Vector3.Lerp(burst, target.position, k);
                    t.position = Vector3.Lerp(a, b, k * k);
                    t.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.7f, k);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
                }
                onArrived?.Invoke();
            }
            finally
            {
                if (coin != null) Destroy(coin.gameObject);
            }
        }

        /// <summary>Back claims too: the reward is never lost by dismissing.</summary>
        public override bool HandleBack()
        {
            Choose(true);
            return true;
        }
    }
}
