using System.Collections.Generic;
using Supono.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    public readonly struct LevelButtonModel
    {
        public readonly string Name;
        public readonly bool Unlocked;
        public readonly bool Completed;

        public LevelButtonModel(string name, bool unlocked, bool completed)
        {
            Name = name;
            Unlocked = unlocked;
            Completed = completed;
        }
    }

    /// <summary>Resolves to the chosen level index, or <see cref="Back"/>.</summary>
    public sealed class LevelSelectWindow : ChoiceWindow<int>
    {
        public const int Back = -1;

        [SerializeField] List<LevelButtonView> buttons = new();
        [SerializeField] Button backButton;

        protected override void Awake()
        {
            base.Awake();
            backButton.onClick.AddListener(() => Choose(Back));
            for (int i = 0; i < buttons.Count; i++)
            {
                int index = i;
                buttons[i].Clicked += () => Choose(index);
            }
        }

        public void Setup(IReadOnlyList<LevelButtonModel> levels)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                bool exists = i < levels.Count;
                buttons[i].gameObject.SetActive(exists);
                if (exists) buttons[i].Bind(i + 1, levels[i]);
            }
        }

        public override bool HandleBack()
        {
            Choose(Back);
            return true;
        }
    }
}
