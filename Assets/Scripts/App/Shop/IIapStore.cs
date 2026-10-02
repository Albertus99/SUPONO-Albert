using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Supono.App.Shop
{
    /// <summary>How a purchase attempt ended, worded for the player.</summary>
    public sealed class PurchaseResult
    {
        PurchaseResult(string productId, bool succeeded, string message)
        {
            ProductId = productId;
            Succeeded = succeeded;
            Message = message;
        }

        public string ProductId { get; }
        public bool Succeeded { get; }
        public string Message { get; }

        public static PurchaseResult Success(string productId) => new(productId, true, string.Empty);
        public static PurchaseResult Failure(string productId, string message) => new(productId, false, message);
    }

    /// <summary>The real-money store (platform store, or the simulated one in the editor).</summary>
    public interface IIapStore
    {
        /// <summary>
        /// A purchase was paid for: grant it. Raised for every completed order, including ones finished
        /// after a restart (interrupted purchases), not only those awaited through <see cref="PurchaseAsync"/>.
        /// </summary>
        event Action<string> Purchased;

        bool IsReady { get; }

        /// <summary>Connects and fetches products. Safe to call repeatedly; returns false if the store is unavailable.</summary>
        UniTask<bool> InitializeAsync(CancellationToken cancellation);

        /// <summary>Localized price, or null if unknown.</summary>
        string PriceOf(string productId);

        /// <summary>Runs the purchase dialog and reports the outcome. Never throws for store failures.</summary>
        UniTask<PurchaseResult> PurchaseAsync(string productId, CancellationToken cancellation);
    }
}
