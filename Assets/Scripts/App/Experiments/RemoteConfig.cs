using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;

namespace Supono.App.Experiments
{
    /// <summary>Server-driven settings.</summary>
    public interface IRemoteConfig
    {
        /// <summary>Fetches the latest values. False when the service can't be reached (cached or no values then).</summary>
        UniTask<bool> FetchAsync(CancellationToken cancellation);

        bool TryGetString(string key, out string value);
    }

    /// <summary>
    /// Unity Remote Config (Unity Gaming Services). Signs in anonymously, so the player id (and with it any
    /// Game Override assignment) stays the same across sessions on this device.
    /// </summary>
    public sealed class UnityRemoteConfig : IRemoteConfig
    {
        // Remote Config targets overrides with these; empty is fine for a percentage rollout.
        struct UserAttributes { }

        struct AppAttributes
        {
            public string appVersion;
            public string platform;
        }

        RuntimeConfig config;

        public async UniTask<bool> FetchAsync(CancellationToken cancellation)
        {
            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                    await UnityServices.InitializeAsync().AsUniTask().AttachExternalCancellation(cancellation);
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync().AsUniTask().AttachExternalCancellation(cancellation);

                var app = new AppAttributes { appVersion = Application.version, platform = Application.platform.ToString() };
                config = await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), app)
                    .AsUniTask().AttachExternalCancellation(cancellation);

                Debug.Log($"[RemoteConfig] Fetched ({config.origin}), assignment {config.assignmentId}.");
                return config.origin == ConfigOrigin.Remote;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[RemoteConfig] Fetch failed, using defaults: {exception.Message}");
                return false;
            }
        }

        public bool TryGetString(string key, out string value)
        {
            value = null;
            if (config == null || !config.HasKey(key)) return false;
            value = config.GetString(key);
            return !string.IsNullOrEmpty(value);
        }
    }
}
