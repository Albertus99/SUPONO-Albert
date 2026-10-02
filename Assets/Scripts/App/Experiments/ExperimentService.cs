using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.App.Analytics;
using Supono.App.Saving;
using UnityEngine;
using VContainer.Unity;

namespace Supono.App.Experiments
{
    /// <summary>Which variant of each A/B test this player is in.</summary>
    public interface IExperimentAssignments
    {
        /// <summary>Completes once assignments are known (Remote Config answered, or timed out to saved/control).</summary>
        UniTask WhenReadyAsync(CancellationToken cancellation);

        IReadOnlyList<Experiment> Experiments { get; }

        string VariantOf(Experiment experiment);

        /// <summary>The player actually saw the experiment's feature. Logged once per session.</summary>
        void ReportExposure(Experiment experiment);
    }

    /// <summary>
    /// Assigns A/B variants from Unity Remote Config at boot. The server is the authority; its answer is saved
    /// so the player stays in the same variant offline. With no answer and nothing saved, the control is used
    /// (and not saved, so the server can still assign later). Each assignment is set as an analytics user
    /// property, so every event can be segmented by variant.
    /// </summary>
    public sealed class ExperimentService : IExperimentAssignments, IStartable, IDisposable
    {
        const string Section = "experiments";
        static readonly TimeSpan FetchTimeout = TimeSpan.FromSeconds(4);

        [Serializable]
        sealed class Data
        {
            public List<Assignment> assignments = new();
        }

        [Serializable]
        sealed class Assignment
        {
            public string experiment;
            public string variant;
        }

        readonly IRemoteConfig remote;
        readonly ISaveService saves;
        readonly IReadOnlyList<Experiment> experiments;
        readonly IAnalytics analytics;
        readonly Dictionary<string, string> variants = new();
        readonly HashSet<string> exposed = new();
        readonly UniTaskCompletionSource ready = new();
        readonly CancellationTokenSource lifetime = new();

        public ExperimentService(IRemoteConfig remote, ISaveService saves, IReadOnlyList<Experiment> experiments, IAnalytics analytics)
        {
            this.remote = remote;
            this.saves = saves;
            this.experiments = experiments;
            this.analytics = analytics;
        }

        public void Start()
        {
            // Saved assignments are usable immediately; the fetch refines them.
            foreach (Assignment saved in saves.Load<Data>(Section).assignments)
                variants[saved.experiment] = saved.variant;
            AssignAsync(lifetime.Token).Forget();
        }

        public UniTask WhenReadyAsync(CancellationToken cancellation) => ready.Task.AttachExternalCancellation(cancellation);

        public IReadOnlyList<Experiment> Experiments => experiments;

        public string VariantOf(Experiment experiment) =>
            variants.TryGetValue(experiment.RemoteKey, out string variant) && experiment.HasVariant(variant) ? variant : experiment.Control;

        public void ReportExposure(Experiment experiment)
        {
            if (exposed.Add(experiment.RemoteKey))
                analytics.Log(new ExperimentExposureEvent(experiment.RemoteKey, VariantOf(experiment)));
        }

        async UniTaskVoid AssignAsync(CancellationToken cancellation)
        {
            bool fetched = false;
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
            {
                timeout.CancelAfter(FetchTimeout);
                try
                {
                    fetched = await remote.FetchAsync(timeout.Token);
                }
                catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
                {
                    Debug.LogWarning("[Experiments] Remote Config timed out; using saved assignments or the control.");
                }
            }

            if (fetched)
            {
                foreach (Experiment experiment in experiments)
                {
                    if (remote.TryGetString(experiment.RemoteKey, out string variant) && experiment.HasVariant(variant))
                        variants[experiment.RemoteKey] = variant;
                    else
                        variants.Remove(experiment.RemoteKey); // no override: back to the control
                }
                Save();
            }

            foreach (Experiment experiment in experiments)
            {
                string variant = VariantOf(experiment);
                analytics.SetUserProperty(UserPropertyName(experiment), variant);
                Debug.Log($"[Experiments] {experiment.RemoteKey} = {variant}{(fetched ? "" : " (offline)")}");
            }
            ready.TrySetResult();
        }

        void Save()
        {
            var data = new Data();
            foreach (KeyValuePair<string, string> pair in variants)
                data.assignments.Add(new Assignment { experiment = pair.Key, variant = pair.Value });
            saves.Store(Section, data);
        }

        // Firebase user property names: up to 24 characters.
        static string UserPropertyName(Experiment experiment)
        {
            string name = "ab_" + experiment.RemoteKey;
            return name.Length <= 24 ? name : name.Substring(0, 24);
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }
    }
}
