using UnityEngine;

namespace Supono.App.Windows
{
    /// <summary>Spins a "working..." icon; unscaled, so it keeps turning while the game is paused.</summary>
    public sealed class UiSpinner : MonoBehaviour
    {
        [SerializeField] float degreesPerSecond = -220f;

        void Update() => transform.Rotate(0f, 0f, degreesPerSecond * Time.unscaledDeltaTime);
    }
}
