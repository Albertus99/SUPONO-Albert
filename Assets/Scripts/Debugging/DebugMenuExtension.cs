using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.App.Extensions;
using Supono.UI;
using UnityEngine;

namespace Supono.Debugging
{
    /// <summary>The gear button on the main menu: opens the debug menu.</summary>
    public sealed class DebugMenuExtension : IMainMenuExtension
    {
        readonly IWindowManager windows;
        readonly DebugSettings debug;

        public DebugMenuExtension(IWindowManager windows, DebugSettings debug, Sprite icon)
        {
            this.windows = windows;
            this.debug = debug;
            Icon = icon;
        }

        public string Label => "Debug";
        public Sprite Icon { get; }

        public async UniTask RunAsync(CancellationToken cancellation)
        {
            // Subscribe in the setup callback (before the window shows) so clicks during the fade-in aren't lost.
            DebugMenuWindow window = null;
            try
            {
                window = await windows.OpenAsync<DebugMenuWindow>(w =>
                {
                    window = w;
                    w.Setup(debug.All);
                    w.ToggleChanged += OnToggleChanged;
                    w.ResetClicked += OnReset;
                }, cancellation);
                await window.WaitForChoiceAsync(cancellation);
            }
            finally
            {
                if (window != null)
                {
                    window.ToggleChanged -= OnToggleChanged;
                    window.ResetClicked -= OnReset;
                }
            }
            await windows.CloseAsync(window, cancellation);

            void OnToggleChanged(DebugToggle toggle, bool on) => debug.Set(toggle, on);

            void OnReset()
            {
                debug.ResetAll();
                window.Refresh();
            }
        }
    }
}
