using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Supono.App.Windows.Controls
{
    /// <summary>
    /// Mobile movement stick. Touch anywhere in this zone and the stick appears under the finger; dragging
    /// feeds a virtual gamepad stick (<see cref="stickControlPath"/>), so gameplay reads it through the normal
    /// "Move" action like any other device. On release it fades back to its resting spot.
    /// </summary>
    public sealed class FloatingJoystick : OnScreenControl, IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler, IPointerUpHandler
    {
        [InputControl(layout = "Vector2")]
        [SerializeField] string stickControlPath = "<Gamepad>/leftStick";

        [SerializeField] RectTransform stickBase;
        [SerializeField] RectTransform knob;
        [SerializeField] CanvasGroup visuals;
        [SerializeField, Tooltip("How far the knob travels for full speed (canvas units).")] float radius = 120f;
        [SerializeField, Range(0f, 1f)] float idleAlpha = 0.35f;

        Vector2 restPosition;
        int pointerId = NoPointer;
        const int NoPointer = int.MinValue;

        protected override string controlPathInternal
        {
            get => stickControlPath;
            set => stickControlPath = value;
        }

        void Awake() => restPosition = stickBase.anchoredPosition;

        protected override void OnEnable()
        {
            base.OnEnable();
            Release();
        }

        protected override void OnDisable()
        {
            Release();
            base.OnDisable();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerId != NoPointer) return; // second finger: leave it for the sprint button
            pointerId = eventData.pointerId;
            stickBase.position = eventData.position; // the stick jumps under the finger
            knob.anchoredPosition = Vector2.zero;
            visuals.alpha = 1f;
        }

        // React from the first pixel instead of waiting for the drag threshold.
        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerId) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(stickBase, eventData.position, eventData.pressEventCamera, out Vector2 local);
            Vector2 offset = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = offset;
            SendValueToControl(offset / radius);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerId) Release();
        }

        void Release()
        {
            pointerId = NoPointer;
            if (knob == null) return;
            knob.anchoredPosition = Vector2.zero;
            stickBase.anchoredPosition = restPosition;
            visuals.alpha = idleAlpha;
            if (control != null) SendValueToControl(Vector2.zero);
        }
    }
}
