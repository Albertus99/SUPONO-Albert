using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Supono.App.Windows
{
    public enum MainMenuChoice
    {
        Play,
        Characters,
        Shop,
        Quit,
        /// <summary>One of the module buttons; see <see cref="MainMenuWindow.ChosenExtension"/>.</summary>
        Extension,
    }

    public readonly struct MenuExtensionButton
    {
        public readonly string Label;
        public readonly Sprite Icon;

        public MenuExtensionButton(string label, Sprite icon)
        {
            Label = label;
            Icon = icon;
        }
    }

    public sealed class MainMenuWindow : ChoiceWindow<MainMenuChoice>
    {
        [SerializeField] Button playButton;
        [SerializeField] Button charactersButton;
        [SerializeField] Button shopButton;
        [SerializeField] Button quitButton;
        [SerializeField] CoinCounter coins;
        [SerializeField, Tooltip("The player's A/B test groups.")] TMP_Text experimentsLabel;

        [Header("Module buttons")]
        [SerializeField] RectTransform extensionBar;
        [SerializeField] Button extensionTemplate;

        readonly List<Button> extensionButtons = new();
        IObjectResolver resolver;

        [Inject]
        public void Construct(IObjectResolver resolver) => this.resolver = resolver;

        /// <summary>Index of the module button chosen with <see cref="MainMenuChoice.Extension"/>.</summary>
        public int ChosenExtension { get; private set; } = -1;

        /// <summary>Where reward coins fly to.</summary>
        public RectTransform CoinTarget => coins.Target;

        protected override void Awake()
        {
            base.Awake();
            extensionTemplate.gameObject.SetActive(false);
            playButton.onClick.AddListener(() => Choose(MainMenuChoice.Play));
            charactersButton.onClick.AddListener(() => Choose(MainMenuChoice.Characters));
            shopButton.onClick.AddListener(() => Choose(MainMenuChoice.Shop));
            quitButton.onClick.AddListener(() => Choose(MainMenuChoice.Quit));
        }

        public void Setup(bool canPlay, int coinTotal, IReadOnlyList<MenuExtensionButton> extensions)
        {
            playButton.interactable = canPlay;
            coins.Set(coinTotal);
            ChosenExtension = -1;

            while (extensionButtons.Count < extensions.Count)
            {
                Button button = resolver.Instantiate(extensionTemplate, extensionBar); // injects its ButtonSound
                int index = extensionButtons.Count;
                button.onClick.AddListener(() =>
                {
                    ChosenExtension = index;
                    Choose(MainMenuChoice.Extension);
                });
                extensionButtons.Add(button);
            }
            for (int i = 0; i < extensionButtons.Count; i++)
            {
                bool used = i < extensions.Count;
                extensionButtons[i].gameObject.SetActive(used);
                if (!used) continue;
                var image = (Image)extensionButtons[i].targetGraphic;
                TMP_Text label = extensionButtons[i].GetComponentInChildren<TMP_Text>(true);
                bool hasIcon = extensions[i].Icon != null;
                if (hasIcon) image.sprite = extensions[i].Icon;
                label.gameObject.SetActive(!hasIcon);
                label.text = extensions[i].Label;
            }
        }

        /// <summary>Lines like "Daily reward: generous"; empty hides the label.</summary>
        public void ShowExperiments(string text)
        {
            experimentsLabel.text = text;
            experimentsLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        public UniTask CountCoinsAsync(int from, int to, CancellationToken cancellation) => coins.CountAsync(from, to, cancellation);
    }
}
