using System;
using System.Collections.Generic;
using UnityEngine;

namespace Supono.App.Experiments
{
    [Serializable]
    public sealed class DailyRewardVariant
    {
        public string id;
        [Tooltip("Coins for day 1..7 of the streak.")]
        public int[] rewards = { 25, 50, 75, 100, 150, 200, 300 };
    }

    /// <summary>
    /// A/B test: does a more generous daily reward ladder improve retention (and does it hurt coin purchases)?
    /// Compare day-1 / day-7 retention and purchase rate between the variants in analytics.
    /// </summary>
    [CreateAssetMenu(menuName = "Supono/Experiments/Daily Reward Experiment", fileName = "DailyRewardExperiment")]
    public sealed class DailyRewardExperiment : Experiment
    {
        [SerializeField] List<DailyRewardVariant> variants = new();

        public override IReadOnlyList<string> VariantIds => variants.ConvertAll(v => v.id);

        public DailyRewardVariant Variant(string id) =>
            variants.Find(v => v.id == id) ?? (variants.Count > 0 ? variants[0] : new DailyRewardVariant());
    }
}
