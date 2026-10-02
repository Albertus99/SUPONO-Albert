using Supono.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    /// <summary>Clear outcome of a purchase: a success card with the coins received, or a failure card with the reason.</summary>
    public sealed class PurchaseResultWindow : ChoiceWindow<bool>
    {
        [SerializeField] GameObject successBadge;
        [SerializeField] GameObject failureBadge;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text messageLabel;
        [SerializeField] GameObject coinsRow;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] Button okButton;

        protected override void Awake()
        {
            base.Awake();
            okButton.onClick.AddListener(() => Choose(true));
        }

        public void ShowSuccess(string packName, int coins)
        {
            Apply(true, "Purchase Complete!", $"Thanks! Your {packName} has been added.");
            coinsRow.SetActive(true);
            coinsLabel.text = $"+{coins:N0}";
        }

        public void ShowFailure(string reason)
        {
            Apply(false, "Purchase Failed", reason);
            coinsRow.SetActive(false);
        }

        void Apply(bool success, string title, string message)
        {
            successBadge.SetActive(success);
            failureBadge.SetActive(!success);
            titleLabel.text = title;
            titleLabel.color = success ? new Color(1f, 0.86f, 0.3f) : new Color(1f, 0.45f, 0.4f);
            messageLabel.text = message;
        }

        public override bool HandleBack()
        {
            Choose(true);
            return true;
        }
    }
}
