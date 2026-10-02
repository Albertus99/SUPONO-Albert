using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    public readonly struct CoinPackModel
    {
        public readonly string Name;
        public readonly int Coins;
        public readonly string Price;
        public readonly string Badge;
        public readonly Sprite Icon;

        public CoinPackModel(string name, int coins, string price, string badge, Sprite icon)
        {
            Name = name;
            Coins = coins;
            Price = price;
            Badge = badge;
            Icon = icon;
        }
    }

    public sealed class CoinPackView : MonoBehaviour
    {
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] TMP_Text priceLabel;
        [SerializeField] Image icon;
        [SerializeField] GameObject badge;
        [SerializeField] TMP_Text badgeLabel;
        [SerializeField] Button buyButton;

        public event Action Clicked;

        void Awake() => buyButton.onClick.AddListener(() => Clicked?.Invoke());

        public RectTransform CoinSource => (RectTransform)icon.transform;

        public void Bind(CoinPackModel model)
        {
            nameLabel.text = model.Name;
            coinsLabel.text = model.Coins.ToString("N0");
            priceLabel.text = model.Price;
            if (model.Icon != null) icon.sprite = model.Icon;
            badge.SetActive(!string.IsNullOrEmpty(model.Badge));
            badgeLabel.text = model.Badge;
        }

        public void SetInteractable(bool interactable) => buyButton.interactable = interactable;
    }
}
