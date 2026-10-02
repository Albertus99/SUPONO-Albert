using Supono.UI;
using UnityEngine;

namespace Supono.App.Windows
{
    /// <summary>Full-screen overlay shown while scenes load. Spins its icon with unscaled time.</summary>
    public sealed class LoadingWindow : Window
    {
        [SerializeField] RectTransform spinner;
        [SerializeField] float spinSpeed = -270f;

        void Update()
        {
            if (spinner != null) spinner.Rotate(0f, 0f, spinSpeed * Time.unscaledDeltaTime);
        }
    }
}
