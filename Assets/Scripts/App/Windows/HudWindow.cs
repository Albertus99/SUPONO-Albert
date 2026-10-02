using System;
using Supono.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    /// <summary>In-level HUD. Passive view: <c>HudPresenter</c> pushes values in.</summary>
    public sealed class HudWindow : Window
    {
        [SerializeField] TMP_Text goalLabel;
        [SerializeField] TMP_Text sizeLabel;
        [SerializeField] RectTransform staminaFill;
        [SerializeField] Graphic staminaFillGraphic;
        [SerializeField] Color staminaColor = new Color(0.35f, 0.85f, 1f);
        [SerializeField] Color exhaustedColor = new Color(1f, 0.35f, 0.3f);
        [SerializeField] Button pauseButton;
        [SerializeField] TMP_Text coinsLabel;

        string shownGoal;
        float shownSize = -1f;
        float shownNeeded = -1f;
        int shownCoins = -1;

        public event Action PauseClicked;

        protected override void Awake()
        {
            base.Awake();
            pauseButton.onClick.AddListener(() => PauseClicked?.Invoke());
        }

        public void SetGoal(string goalName)
        {
            if (goalName == shownGoal) return;
            shownGoal = goalName;
            goalLabel.text = goalName != null ? $"Eat the {goalName}!" : "Eat the biggest animal!";
        }

        public void SetSizes(float playerSize, float neededSize)
        {
            if (Mathf.Approximately(playerSize, shownSize) && Mathf.Approximately(neededSize, shownNeeded)) return;
            shownSize = playerSize;
            shownNeeded = neededSize;
            sizeLabel.text = neededSize > 0f
                ? $"Size {playerSize:0.00} / {neededSize:0.00}"
                : $"Size {playerSize:0.00}";
        }

        public void SetStamina(float normalized, bool exhausted)
        {
            staminaFill.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
            staminaFillGraphic.color = exhausted ? exhaustedColor : staminaColor;
        }

        public void SetCoins(int coins)
        {
            if (coins == shownCoins) return;
            shownCoins = coins;
            coinsLabel.text = coins.ToString();
        }
    }
}
