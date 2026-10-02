using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Supono.App.Extensions
{
    /// <summary>An extra main menu button contributed by a module. The menu is closed while it runs.</summary>
    public interface IMainMenuExtension
    {
        string Label { get; }

        /// <summary>Icon for the button; null shows <see cref="Label"/> as text.</summary>
        Sprite Icon { get; }

        UniTask RunAsync(CancellationToken cancellation);
    }
}
