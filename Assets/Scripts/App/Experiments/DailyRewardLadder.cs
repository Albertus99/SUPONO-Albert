using System.Collections.Generic;

namespace Supono.App.Experiments
{
    /// <summary>The coins for each day of the daily reward streak.</summary>
    public interface IDailyRewardLadder
    {
        IReadOnlyList<int> Rewards { get; }

        /// <summary>The A/B group this ladder comes from.</summary>
        string Group { get; }

        /// <summary>The ladder was shown to the player.</summary>
        void ReportShown();
    }

    /// <summary>The ladder of the player's variant in the <see cref="DailyRewardExperiment"/>.</summary>
    public sealed class ExperimentDailyRewardLadder : IDailyRewardLadder
    {
        readonly DailyRewardExperiment experiment;
        readonly IExperimentAssignments assignments;

        public ExperimentDailyRewardLadder(DailyRewardExperiment experiment, IExperimentAssignments assignments)
        {
            this.experiment = experiment;
            this.assignments = assignments;
        }

        public IReadOnlyList<int> Rewards => experiment.Variant(Group).rewards;

        public string Group => assignments.VariantOf(experiment);

        public void ReportShown() => assignments.ReportExposure(experiment);
    }
}
