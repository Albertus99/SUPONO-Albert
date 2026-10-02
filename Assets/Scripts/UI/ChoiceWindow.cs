using System.Threading;
using Cysharp.Threading.Tasks;

namespace Supono.UI
{
    /// <summary>A window that resolves to a single choice made by the player, e.g. Resume / Restart / Quit.</summary>
    public abstract class ChoiceWindow<TChoice> : Window
    {
        UniTaskCompletionSource<TChoice> pending = new();

        public UniTask<TChoice> WaitForChoiceAsync(CancellationToken cancellation = default) =>
            pending.Task.AttachExternalCancellation(cancellation);

        protected void Choose(TChoice choice) => pending.TrySetResult(choice);

        protected override void OnOpened() => pending = new UniTaskCompletionSource<TChoice>();

        protected override void OnClosed() => pending.TrySetCanceled();
    }
}
