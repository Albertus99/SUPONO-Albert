using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    /// <summary>
    /// Coin packs for real money. Passive view: the menu flow runs purchases and shows the outcome.
    /// While the store is busy the packs are locked and a status line says what's happening.
    /// </summary>
    public sealed class CoinShopWindow : ChoiceWindow<bool>
    {
        [SerializeField] List<CoinPackView> packs = new();
        [SerializeField] CoinCounter coins;
        [SerializeField] Button backButton;
        [SerializeField] GameObject busyOverlay;
        [SerializeField] TMP_Text busyLabel;
        [SerializeField] TMP_Text footnote;

        public event Action<int> PackClicked;

        public CoinCounter Coins => coins;

        protected override void Awake()
        {
            base.Awake();
            backButton.onClick.AddListener(() => Choose(true));
            for (int i = 0; i < packs.Count; i++)
            {
                int index = i;
                packs[i].Clicked += () => PackClicked?.Invoke(index);
            }
        }

        public void Bind(int coinTotal, IReadOnlyList<CoinPackModel> models, string note)
        {
            coins.Set(coinTotal);
            footnote.text = note;
            for (int i = 0; i < packs.Count; i++)
            {
                bool used = i < models.Count;
                packs[i].gameObject.SetActive(used);
                if (used) packs[i].Bind(models[i]);
            }
        }

        public RectTransform PackIcon(int index) => packs[index].CoinSource;

        /// <summary>Locks the shop with a status message (null unlocks).</summary>
        public void SetBusy(string status)
        {
            bool busy = status != null;
            busyOverlay.SetActive(busy);
            busyLabel.text = status ?? string.Empty;
            backButton.interactable = !busy;
            foreach (CoinPackView pack in packs) pack.SetInteractable(!busy);
        }

        public override bool HandleBack()
        {
            if (!busyOverlay.activeSelf) Choose(true);
            return true;
        }

        public UniTask CountCoinsAsync(int from, int to, CancellationToken cancellation) => coins.CountAsync(from, to, cancellation);
    }
}
