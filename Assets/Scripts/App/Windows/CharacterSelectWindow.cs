using System;
using System.Collections.Generic;
using Supono.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Supono.App.Windows
{
    /// <summary>
    /// Scrollable grid of characters with spinnable 3D previews. Passive view: the menu flow builds the
    /// cards (<see cref="Setup"/>), handles <see cref="CardClicked"/> / spins and re-binds.
    /// </summary>
    public sealed class CharacterSelectWindow : ChoiceWindow<bool>
    {
        [SerializeField] ScrollRect scroll;
        [SerializeField] RectTransform content;
        [SerializeField] CharacterCardView cardTemplate;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] Button backButton;

        readonly List<CharacterCardView> cards = new();
        IObjectResolver resolver;

        /// <summary>Card index the player wants to unlock or select.</summary>
        public event Action<int> CardClicked;

        /// <summary>(card index, degrees) while a preview is dragged.</summary>
        public event Action<int, float> CardSpun;

        public event Action<int> CardSpinReleased;

        [Inject]
        public void Construct(IObjectResolver resolver) => this.resolver = resolver;

        protected override void Awake()
        {
            base.Awake();
            cardTemplate.gameObject.SetActive(false);
            backButton.onClick.AddListener(() => Choose(true));
        }

        /// <summary>Creates one card per character, each showing its tile of the shared preview texture.</summary>
        public void Setup(Texture previews, IReadOnlyList<Rect> tiles)
        {
            while (cards.Count < tiles.Count)
            {
                // Through the resolver so the card's components (e.g. ButtonSound) get injected.
                CharacterCardView card = resolver.Instantiate(cardTemplate, content);
                int index = cards.Count;
                card.Clicked += () => CardClicked?.Invoke(index);
                card.Spun += degrees => CardSpun?.Invoke(index, degrees);
                card.SpinReleased += () => CardSpinReleased?.Invoke(index);
                cards.Add(card);
            }
            for (int i = 0; i < cards.Count; i++)
            {
                bool used = i < tiles.Count;
                cards[i].gameObject.SetActive(used);
                if (used) cards[i].SetPreview(previews, tiles[i]);
            }
            scroll.verticalNormalizedPosition = 1f;
        }

        public void Bind(int coins, IReadOnlyList<CharacterCardModel> models)
        {
            coinsLabel.text = coins.ToString();
            for (int i = 0; i < models.Count && i < cards.Count; i++) cards[i].Bind(models[i]);
        }

        public override bool HandleBack()
        {
            Choose(true);
            return true;
        }
    }
}
