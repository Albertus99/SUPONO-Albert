using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Supono.UI
{
    public interface IWindowManager
    {
        /// <summary>Back/Escape pressed and no open window handled it.</summary>
        event Action BackUnhandled;

        /// <summary>
        /// Opens (or re-opens) the window of type <typeparamref name="T"/>. <paramref name="setup"/> runs before it shows.
        /// Cancelling <paramref name="cancellation"/> stops the caller waiting (throws <see cref="OperationCanceledException"/>);
        /// the window itself still finishes showing.
        /// </summary>
        UniTask<T> OpenAsync<T>(Action<T> setup = null, CancellationToken cancellation = default) where T : Window;

        /// <inheritdoc cref="OpenAsync{T}"/>
        UniTask CloseAsync(Window window, CancellationToken cancellation = default);
        UniTask CloseAsync<T>(CancellationToken cancellation = default) where T : Window;
        void CloseAll();
        bool TryGetOpen<T>(out T window) where T : Window;
    }

    /// <summary>
    /// Lives in the project scope. Instantiates windows from the <see cref="WindowCatalog"/> into the
    /// persistent <see cref="UIRoot"/>, caches them, and routes Back/Escape to the top-most window.
    /// </summary>
    public sealed class WindowManager : IWindowManager, ITickable
    {
        readonly IObjectResolver resolver;
        readonly UIRoot root;
        readonly WindowCatalog catalog;
        readonly IReadOnlyList<IWindowSource> moduleWindows;
        readonly InputAction back;
        readonly Dictionary<Type, Window> instances = new();
        readonly List<Window> open = new();

        public event Action BackUnhandled;

        /// <param name="catalog">The game's windows.</param>
        /// <param name="moduleWindows">Windows contributed by optional modules (may be empty).</param>
        public WindowManager(IObjectResolver resolver, UIRoot root, WindowCatalog catalog, IReadOnlyList<IWindowSource> moduleWindows, InputActionAsset input)
        {
            this.resolver = resolver;
            this.root = root;
            this.catalog = catalog;
            this.moduleWindows = moduleWindows;
            InputActionMap uiMap = input.FindActionMap("UI", throwIfNotFound: true);
            back = uiMap.FindAction("Cancel", throwIfNotFound: true);
            uiMap.Enable();
        }

        public async UniTask<T> OpenAsync<T>(Action<T> setup = null, CancellationToken cancellation = default) where T : Window
        {
            cancellation.ThrowIfCancellationRequested();
            T window = GetOrCreate<T>();
            setup?.Invoke(window);
            open.Remove(window);
            open.Add(window);
            await window.ShowAsync().AttachExternalCancellation(cancellation);
            return window;
        }

        public UniTask CloseAsync(Window window, CancellationToken cancellation = default)
        {
            if (window == null || !open.Remove(window)) return UniTask.CompletedTask;
            return window.HideAsync().AttachExternalCancellation(cancellation);
        }

        public UniTask CloseAsync<T>(CancellationToken cancellation = default) where T : Window =>
            TryGetOpen(out T window) ? CloseAsync(window, cancellation) : UniTask.CompletedTask;

        public void CloseAll()
        {
            foreach (Window window in open.ToArray()) CloseAsync(window).Forget();
        }

        public bool TryGetOpen<T>(out T window) where T : Window
        {
            window = instances.TryGetValue(typeof(T), out Window cached) && cached != null && cached.IsOpen
                ? (T)cached
                : null;
            return window != null;
        }

        void ITickable.Tick()
        {
            if (!back.WasPressedThisFrame()) return;
            Window top = TopWindow();
            if (top != null && top.Layer == WindowLayer.Overlay) return; // e.g. loading screen swallows input
            if (top == null || !top.HandleBack()) BackUnhandled?.Invoke();
        }

        /// <summary>Highest layer wins; within a layer, the most recently opened.</summary>
        Window TopWindow()
        {
            Window top = null;
            foreach (Window window in open)
            {
                if (window.Layer == WindowLayer.Hud) continue;
                if (top == null || window.Layer >= top.Layer) top = window;
            }
            return top;
        }

        T GetOrCreate<T>() where T : Window
        {
            if (instances.TryGetValue(typeof(T), out Window cached) && cached != null) return (T)cached;

            T prefab = FindPrefab<T>()
                       ?? throw new InvalidOperationException($"{typeof(T).Name} is missing from the WindowCatalog and module windows.");
            T instance = resolver.Instantiate(prefab, root.GetLayer(prefab.Layer));
            instance.gameObject.SetActive(false);
            instances[typeof(T)] = instance;
            return instance;
        }

        T FindPrefab<T>() where T : Window
        {
            T prefab = catalog.Find<T>();
            if (prefab != null) return prefab;
            foreach (IWindowSource source in moduleWindows)
            {
                prefab = source.Find<T>();
                if (prefab != null) return prefab;
            }
            return null;
        }
    }
}
