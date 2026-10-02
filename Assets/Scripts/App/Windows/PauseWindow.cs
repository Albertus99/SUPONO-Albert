using Supono.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    public enum PauseChoice
    {
        Resume,
        Restart,
        MainMenu,
    }

    public sealed class PauseWindow : ChoiceWindow<PauseChoice>
    {
        [SerializeField] Button resumeButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button menuButton;

        protected override void Awake()
        {
            base.Awake();
            resumeButton.onClick.AddListener(() => Choose(PauseChoice.Resume));
            restartButton.onClick.AddListener(() => Choose(PauseChoice.Restart));
            menuButton.onClick.AddListener(() => Choose(PauseChoice.MainMenu));
        }

        public override bool HandleBack()
        {
            Choose(PauseChoice.Resume);
            return true;
        }
    }
}
