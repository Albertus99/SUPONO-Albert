using System.Collections.Generic;
using UnityEngine;

namespace Supono.UI
{
    /// <summary>Somewhere window prefabs can be found by type.</summary>
    public interface IWindowSource
    {
        T Find<T>() where T : Window;
    }

    /// <summary>All window prefabs the <see cref="WindowManager"/> can open, looked up by type.</summary>
    [CreateAssetMenu(menuName = "Supono/Window Catalog", fileName = "WindowCatalog")]
    public sealed class WindowCatalog : ScriptableObject, IWindowSource
    {
        [SerializeField] List<Window> windows = new();

        public T Find<T>() where T : Window
        {
            foreach (Window window in windows)
            {
                if (window is T typed) return typed;
            }
            return null;
        }
    }
}
