using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.App.Windows;
using Supono.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Supono.App.Flow
{
    /// <summary>
    /// Swaps the active scene behind a loading screen. Scene-level windows are closed first.
    /// Transitions run on the project scope's lifetime, not the caller's: the scene that requests
    /// a load is about to be destroyed, but the load must still finish. Disposing the project scope
    /// (quitting / leaving play mode) cancels them.
    /// </summary>
    public sealed class SceneLoader : IDisposable
    {
        readonly IWindowManager windows;
        readonly CancellationTokenSource lifetime = new();
        bool loading;

        public SceneLoader(IWindowManager windows) => this.windows = windows;

        public async UniTask LoadAsync(string scenePath)
        {
            if (loading) return;
            loading = true;
            CancellationToken cancellation = lifetime.Token;
            try
            {
                windows.CloseAll();
                await windows.OpenAsync<LoadingWindow>(cancellation: cancellation);
                Time.timeScale = 1f;
                await SceneManager.LoadSceneAsync(scenePath).ToUniTask(cancellationToken: cancellation);
                await windows.CloseAsync<LoadingWindow>(cancellation);
            }
            finally
            {
                loading = false;
            }
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }
    }
}
