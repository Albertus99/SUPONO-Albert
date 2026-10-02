namespace Supono.App.Analytics
{
    /// <summary>A level began (Firebase recommended event "level_start").</summary>
    public sealed class LevelStartEvent : AnalyticsEvent
    {
        readonly int levelNumber;
        readonly string levelName;
        readonly string character;

        public LevelStartEvent(int levelNumber, string levelName, string character)
        {
            this.levelNumber = levelNumber;
            this.levelName = levelName;
            this.character = character;
        }

        public override string Name => "level_start";

        public override void Describe(IEventParameters parameters)
        {
            parameters.Add("level_name", levelName);
            parameters.Add("level_number", levelNumber);
            parameters.Add("character", character);
        }
    }

    /// <summary>A level ended (Firebase recommended event "level_end").</summary>
    public sealed class LevelEndEvent : AnalyticsEvent
    {
        readonly int levelNumber;
        readonly string levelName;
        readonly bool success;
        readonly int coins;
        readonly double seconds;

        public LevelEndEvent(int levelNumber, string levelName, bool success, int coins, double seconds)
        {
            this.levelNumber = levelNumber;
            this.levelName = levelName;
            this.success = success;
            this.coins = coins;
            this.seconds = seconds;
        }

        public override string Name => "level_end";

        public override void Describe(IEventParameters parameters)
        {
            parameters.Add("level_name", levelName);
            parameters.Add("level_number", levelNumber);
            parameters.Add("success", success ? "true" : "false");
            parameters.Add("coins", coins);
            parameters.Add("duration_seconds", seconds);
        }
    }

    /// <summary>The player tapped a coin pack in the shop, before the store dialog.</summary>
    public sealed class PurchaseClickEvent : AnalyticsEvent
    {
        readonly string productId;
        readonly string price;

        public PurchaseClickEvent(string productId, string price)
        {
            this.productId = productId;
            this.price = price;
        }

        public override string Name => "purchase_click";

        public override void Describe(IEventParameters parameters)
        {
            parameters.Add("item_id", productId);
            parameters.Add("price", price);
        }
    }

    /// <summary>How a purchase attempt ended.</summary>
    public sealed class PurchaseResultEvent : AnalyticsEvent
    {
        readonly string productId;
        readonly bool success;
        readonly string reason;

        public PurchaseResultEvent(string productId, bool success, string reason)
        {
            this.productId = productId;
            this.success = success;
            this.reason = reason;
        }

        public override string Name => success ? "purchase_success" : "purchase_failed";

        public override void Describe(IEventParameters parameters)
        {
            parameters.Add("item_id", productId);
            if (!success) parameters.Add("reason", reason);
        }
    }

    /// <summary>The daily login reward was claimed.</summary>
    public sealed class DailyRewardClaimEvent : AnalyticsEvent
    {
        readonly int day;
        readonly int coins;

        public DailyRewardClaimEvent(int day, int coins)
        {
            this.day = day;
            this.coins = coins;
        }

        public override string Name => "daily_reward_claim";

        public override void Describe(IEventParameters parameters)
        {
            parameters.Add("streak_day", day);
            parameters.Add("coins", coins);
        }
    }

    /// <summary>A character was unlocked with coins (Firebase recommended event "unlock_achievement" is for achievements, so a custom one).</summary>
    public sealed class CharacterUnlockEvent : AnalyticsEvent
    {
        readonly string character;
        readonly int price;

        public CharacterUnlockEvent(string character, int price)
        {
            this.character = character;
            this.price = price;
        }

        public override string Name => "character_unlock";

        public override void Describe(IEventParameters parameters)
        {
            parameters.Add("character", character);
            parameters.Add("price", price);
        }
    }

    /// <summary>The player saw the feature an A/B test changes (the moment they enter the experiment's analysis).</summary>
    public sealed class ExperimentExposureEvent : AnalyticsEvent
    {
        readonly string experiment;
        readonly string variant;

        public ExperimentExposureEvent(string experiment, string variant)
        {
            this.experiment = experiment;
            this.variant = variant;
        }

        public override string Name => "experiment_exposure";

        public override void Describe(IEventParameters parameters)
        {
            parameters.Add("experiment", experiment);
            parameters.Add("variant", variant);
        }
    }

    /// <summary>A character was chosen to play (Firebase recommended event "select_item").</summary>
    public sealed class CharacterSelectEvent : AnalyticsEvent
    {
        readonly string character;

        public CharacterSelectEvent(string character) => this.character = character;

        public override string Name => "select_item";

        public override void Describe(IEventParameters parameters)
        {
            parameters.Add("item_list_id", "characters");
            parameters.Add("item_id", character);
        }
    }
}
