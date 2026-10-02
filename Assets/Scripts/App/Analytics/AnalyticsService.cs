using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if SUPONO_FIREBASE
using Firebase;
using Firebase.Extensions;
using FirebaseAnalyticsApi = Firebase.Analytics.FirebaseAnalytics;
using Parameter = Firebase.Analytics.Parameter;
#endif

namespace Supono.App.Analytics
{
    public interface IAnalytics
    {
        void Log(AnalyticsEvent analyticsEvent);
    }

    /// <summary>Writes events to the console. The backend when Firebase isn't installed (editor, tests).</summary>
    public sealed class ConsoleAnalytics : IAnalytics
    {
        public void Log(AnalyticsEvent analyticsEvent)
        {
            var parameters = new ParameterDictionary();
            analyticsEvent.Describe(parameters);

            var line = new StringBuilder("[Analytics] ").Append(analyticsEvent.Name);
            foreach (KeyValuePair<string, object> pair in parameters.Values) line.Append(' ').Append(pair.Key).Append('=').Append(pair.Value);
            Debug.Log(line.ToString());
        }
    }

#if SUPONO_FIREBASE
    /// <summary>
    /// Firebase Analytics backend. Compiled in automatically when the com.google.firebase.analytics package
    /// is installed (asmdef version define), with google-services.json / GoogleService-Info.plist in Assets.
    /// Events logged before Firebase finishes its dependency check are queued, then flushed.
    /// </summary>
    public sealed class FirebaseAnalytics : IAnalytics
    {
        readonly Queue<AnalyticsEvent> pending = new();
        readonly ConsoleAnalytics console = new();
        bool ready;
        bool unavailable;

        public FirebaseAnalytics()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                try
                {
                    if (task.Result != DependencyStatus.Available) throw new InvalidOperationException(task.Result.ToString());
                    _ = FirebaseApp.DefaultInstance; // throws without google-services.json / GoogleService-Info.plist
                    FirebaseAnalyticsApi.SetAnalyticsCollectionEnabled(true);
                    ready = true;
                    while (pending.Count > 0) Send(pending.Dequeue());
                }
                catch (Exception exception)
                {
                    // No backend configured (e.g. in the editor): events keep going to the console only.
                    unavailable = true;
                    pending.Clear();
                    Debug.LogWarning($"[Analytics] Firebase not configured, logging to the console only: {exception.Message}");
                }
            });
        }

        public void Log(AnalyticsEvent analyticsEvent)
        {
            console.Log(analyticsEvent);
            if (ready) Send(analyticsEvent);
            else if (!unavailable) pending.Enqueue(analyticsEvent);
        }

        static void Send(AnalyticsEvent analyticsEvent)
        {
            var parameters = new FirebaseParameters();
            analyticsEvent.Describe(parameters);
            FirebaseAnalyticsApi.LogEvent(analyticsEvent.Name, parameters.ToArray());
        }

        sealed class FirebaseParameters : IEventParameters
        {
            readonly List<Parameter> list = new();

            public void Add(string name, string value) => list.Add(new Parameter(name, value));
            public void Add(string name, long value) => list.Add(new Parameter(name, value));
            public void Add(string name, double value) => list.Add(new Parameter(name, value));

            public Parameter[] ToArray() => list.ToArray();
        }
    }
#endif
}
