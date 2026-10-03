using UnityEngine;
using UnityEngine.InputSystem;

namespace Supono.App.Windows.Controls
{
    /// <summary>
    /// Shows the on-screen controls on phones and tablets (and in the editor, so they can be tried with the mouse);
    /// hides them on desktop builds, where the keyboard is used.
    /// </summary>
    public sealed class TouchControls : MonoBehaviour
    {
        public static bool Wanted => Application.isMobilePlatform || Application.isEditor || Touchscreen.current != null;

        void Awake() => gameObject.SetActive(Wanted);

        /// <summary>How to move, worded for the current controls.</summary>
        public static string Hint => Wanted
            ? "Drag anywhere to move   ·   Hold SPRINT to run"
            : "WASD move   ·   Shift sprint   ·   Esc pause";
    }
}
