using System.Collections.Generic;
using UnityEngine;

namespace Supono.App.Experiments
{
    /// <summary>
    /// One A/B test. The variant a player gets is a string value of <see cref="RemoteKey"/> in Unity Remote Config;
    /// a Remote Config Game Override (with a percentage rollout) gives part of the players the test variant.
    /// The first variant is the control: used when Remote Config is unreachable or returns an unknown value.
    /// </summary>
    public abstract class Experiment : ScriptableObject
    {
        [SerializeField, Tooltip("Remote Config key holding the variant id; also the analytics name of the experiment.")]
        string remoteKey;

        [SerializeField, Tooltip("Shown in the UI next to the player's group.")]
        string displayName;

        public string RemoteKey => remoteKey;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? remoteKey : displayName;

        /// <summary>Variant ids; the first is the control.</summary>
        public abstract IReadOnlyList<string> VariantIds { get; }

        public string Control => VariantIds.Count > 0 ? VariantIds[0] : string.Empty;

        public bool HasVariant(string id)
        {
            foreach (string variant in VariantIds)
            {
                if (variant == id) return true;
            }
            return false;
        }
    }
}
