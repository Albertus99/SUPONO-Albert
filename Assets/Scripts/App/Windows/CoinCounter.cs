using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Supono.App.Windows
{
    /// <summary>Coin icon + amount pill. Can count up with a little punch for reward feedback.</summary>
    public sealed class CoinCounter : MonoBehaviour
    {
        [SerializeField] TMP_Text amountLabel;

        /// <summary>Where reward coins fly to.</summary>
        public RectTransform Target => (RectTransform)transform;

        public void Set(int coins) => amountLabel.text = coins.ToString();

        public async UniTask CountAsync(int from, int to, CancellationToken cancellation)
        {
            const float duration = 0.6f;
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                float k = time / duration;
                amountLabel.text = Mathf.RoundToInt(Mathf.Lerp(from, to, k)).ToString();
                transform.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * 0.15f);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
            }
            Set(to);
            transform.localScale = Vector3.one;
        }
    }
}
