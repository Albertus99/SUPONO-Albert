using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Supono.UI
{
    /// <summary>Sorting layers of the UI root, bottom to top.</summary>
    public enum WindowLayer
    {
        Hud,
        Screen,
        Popup,
        Overlay,
    }

    /// <summary>
    /// Base for everything the <see cref="IWindowManager"/> opens. Windows are instantiated once,
    /// cached, and faded in/out with unscaled time so they work while the game is paused.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Window : MonoBehaviour
    {
        [SerializeField] WindowLayer layer = WindowLayer.Screen;
        [SerializeField, Min(0f)] float fadeDuration = 0.15f;

        CanvasGroup group;
        int fadeVersion;

        public WindowLayer Layer => layer;
        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            SetInteractive(false);
        }

        internal async UniTask ShowAsync()
        {
            IsOpen = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            SetInteractive(true);
            OnOpened();
            await FadeAsync(1f);
        }

        internal async UniTask HideAsync()
        {
            IsOpen = false;
            SetInteractive(false);
            OnClosed();
            await FadeAsync(0f);
            if (!IsOpen) gameObject.SetActive(false);
        }

        /// <summary>Back/Escape pressed while this is the top window. Return true if handled.</summary>
        public virtual bool HandleBack() => false;

        protected virtual void OnOpened() { }
        protected virtual void OnClosed() { }

        /// <summary>
        /// Only toggles raycast blocking: flipping <see cref="CanvasGroup.interactable"/> would render every
        /// button in its disabled tint while the window fades.
        /// </summary>
        void SetInteractive(bool interactive) => group.blocksRaycasts = interactive;

        async UniTask FadeAsync(float target)
        {
            int version = ++fadeVersion;
            float start = group.alpha;
            var token = destroyCancellationToken;
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                if (await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow()) return;
                if (version != fadeVersion) return; // superseded by a newer show/hide
            }
            group.alpha = target;
        }
    }
}
