using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    public sealed class LevelButtonView : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TMP_Text numberLabel;
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] GameObject lockIcon;
        [SerializeField] GameObject completedIcon;
        [SerializeField] CanvasGroup content;

        public event Action Clicked;

        void Awake() => button.onClick.AddListener(() => Clicked?.Invoke());

        public void Bind(int number, LevelButtonModel model)
        {
            numberLabel.text = number.ToString();
            nameLabel.text = model.Name;
            button.interactable = model.Unlocked;
            lockIcon.SetActive(!model.Unlocked);
            completedIcon.SetActive(model.Completed);
            content.alpha = model.Unlocked ? 1f : 0.45f;
        }
    }
}
