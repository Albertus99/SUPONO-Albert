using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Supono.App.Windows
{
    public enum CharacterCardState
    {
        Locked,
        Owned,
        Selected,
    }

    public readonly struct CharacterCardModel
    {
        public readonly string Name;
        public readonly string Description;
        public readonly string Stats;
        public readonly int Price;
        public readonly CharacterCardState State;
        public readonly bool Affordable;

        public CharacterCardModel(string name, string description, string stats, int price, CharacterCardState state, bool affordable)
        {
            Name = name;
            Description = description;
            Stats = stats;
            Price = price;
            State = state;
            Affordable = affordable;
        }
    }

    /// <summary>
    /// One character in the selection grid: live 3D preview, stats and an unlock/select button.
    /// Dragging sideways on the preview spins the model; dragging up/down is passed on to the
    /// scroll view, so the grid still scrolls from anywhere.
    /// </summary>
    public sealed class CharacterCardView : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        const float DegreesPerPixel = 0.6f;

        [SerializeField] RawImage preview;
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] TMP_Text descriptionLabel;
        [SerializeField] TMP_Text statsLabel;
        [SerializeField] Button actionButton;
        [SerializeField] Image actionBackground;
        [SerializeField] TMP_Text actionLabel;
        [SerializeField] GameObject priceIcon;
        [SerializeField] GameObject selectedHighlight;
        [SerializeField] GameObject lockIcon;

        [Header("Button skins")]
        [SerializeField] Sprite unlockSprite;
        [SerializeField] Sprite selectSprite;
        [SerializeField] Sprite selectedSprite;

        bool spinning;
        Transform scrollTarget;

        public event Action Clicked;

        /// <summary>Degrees to spin the preview by (drag step).</summary>
        public event Action<float> Spun;

        /// <summary>The drag ended: let the model keep its momentum.</summary>
        public event Action SpinReleased;

        void Awake() => actionButton.onClick.AddListener(() => Clicked?.Invoke());

        public void SetPreview(Texture texture, Rect tile)
        {
            preview.texture = texture;
            preview.uvRect = tile;
        }

        public void Bind(CharacterCardModel model)
        {
            nameLabel.text = model.Name;
            descriptionLabel.text = model.Description;
            statsLabel.text = model.Stats;
            selectedHighlight.SetActive(model.State == CharacterCardState.Selected);
            lockIcon.SetActive(model.State == CharacterCardState.Locked);

            switch (model.State)
            {
                case CharacterCardState.Locked:
                    actionBackground.sprite = unlockSprite;
                    actionLabel.text = model.Price.ToString();
                    priceIcon.SetActive(true);
                    actionButton.interactable = model.Affordable;
                    break;
                case CharacterCardState.Owned:
                    actionBackground.sprite = selectSprite;
                    actionLabel.text = "Select";
                    priceIcon.SetActive(false);
                    actionButton.interactable = true;
                    break;
                case CharacterCardState.Selected:
                    actionBackground.sprite = selectedSprite;
                    actionLabel.text = "Selected";
                    priceIcon.SetActive(false);
                    actionButton.interactable = false;
                    break;
            }
        }

        // ---------------------------------------------------------------- drag: spin or scroll

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            scrollTarget = transform.parent;
            ExecuteEvents.ExecuteHierarchy(scrollTarget.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Decide once per gesture: mostly sideways = spin, otherwise scroll the grid.
            spinning = Mathf.Abs(eventData.delta.x) >= Mathf.Abs(eventData.delta.y);
            if (spinning) Spin(eventData);
            else ExecuteEvents.ExecuteHierarchy(scrollTarget.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (spinning) Spin(eventData);
            else ExecuteEvents.ExecuteHierarchy(scrollTarget.gameObject, eventData, ExecuteEvents.dragHandler);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (spinning) SpinReleased?.Invoke();
            else ExecuteEvents.ExecuteHierarchy(scrollTarget.gameObject, eventData, ExecuteEvents.endDragHandler);
            spinning = false;
        }

        void Spin(PointerEventData eventData) => Spun?.Invoke(-eventData.delta.x * DegreesPerPixel);
    }
}
