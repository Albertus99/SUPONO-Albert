using Supono.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    public enum LevelResultChoice
    {
        Retry,
        NextLevel,
        MainMenu,
    }

    public sealed class LevelResultWindow : ChoiceWindow<LevelResultChoice>
    {
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text messageLabel;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] GameObject victoryBanner;
        [SerializeField] GameObject defeatBanner;
        [SerializeField] Button retryButton;
        [SerializeField] Button nextButton;
        [SerializeField] Button menuButton;

        protected override void Awake()
        {
            base.Awake();
            retryButton.onClick.AddListener(() => Choose(LevelResultChoice.Retry));
            nextButton.onClick.AddListener(() => Choose(LevelResultChoice.NextLevel));
            menuButton.onClick.AddListener(() => Choose(LevelResultChoice.MainMenu));
        }

        public void Setup(bool won, string title, string message, bool hasNextLevel, int coinsEarned)
        {
            titleLabel.text = title;
            messageLabel.text = message;
            coinsLabel.text = $"+{coinsEarned}";
            if (victoryBanner != null) victoryBanner.SetActive(won);
            if (defeatBanner != null) defeatBanner.SetActive(!won);
            nextButton.gameObject.SetActive(hasNextLevel);
        }

        public override bool HandleBack()
        {
            Choose(LevelResultChoice.MainMenu);
            return true;
        }
    }
}
