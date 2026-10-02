using Supono.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    public enum LevelIntroChoice
    {
        Start,
        MainMenu,
    }

    public sealed class LevelIntroWindow : ChoiceWindow<LevelIntroChoice>
    {
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] Button startButton;
        [SerializeField] Button menuButton;

        protected override void Awake()
        {
            base.Awake();
            startButton.onClick.AddListener(() => Choose(LevelIntroChoice.Start));
            menuButton.onClick.AddListener(() => Choose(LevelIntroChoice.MainMenu));
        }

        public void Setup(string title, string body)
        {
            titleLabel.text = title;
            bodyLabel.text = body;
        }

        public override bool HandleBack()
        {
            Choose(LevelIntroChoice.MainMenu);
            return true;
        }
    }
}
