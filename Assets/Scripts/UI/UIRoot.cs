using System;
using UnityEngine;

namespace Supono.UI
{
    /// <summary>Persistent canvas holding one container per <see cref="WindowLayer"/>.</summary>
    public sealed class UIRoot : MonoBehaviour
    {
        [SerializeField] RectTransform hudLayer;
        [SerializeField] RectTransform screenLayer;
        [SerializeField] RectTransform popupLayer;
        [SerializeField] RectTransform overlayLayer;

        public RectTransform GetLayer(WindowLayer layer) => layer switch
        {
            WindowLayer.Hud => hudLayer,
            WindowLayer.Screen => screenLayer,
            WindowLayer.Popup => popupLayer,
            WindowLayer.Overlay => overlayLayer,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null),
        };
    }
}
