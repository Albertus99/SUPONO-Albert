using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Supono.App.Shop
{
    /// <summary>
    /// <see cref="IIapStore"/> on Unity IAP 5 (StoreController). In the editor and on platforms without a
    /// real store Unity IAP uses its Fake Store: a simulated purchase dialog where Buy succeeds and Cancel
    /// fails, so the full flow (dialog, success, failure, granting) runs without any store account.
    /// Orders are confirmed only after <see cref="Purchased"/> has granted them.
    /// </summary>
    public sealed class UnityIapStore : IIapStore, IDisposable
    {
        readonly CoinPackCatalog catalog;
        readonly Dictionary<string, UniTaskCompletionSource<PurchaseResult>> inFlight = new();
        StoreController store;
        UniTaskCompletionSource<bool> initialization;

        public event Action<string> Purchased;

        public UnityIapStore(CoinPackCatalog catalog) => this.catalog = catalog;

        public bool IsReady { get; private set; }

        public UniTask<bool> InitializeAsync(CancellationToken cancellation)
        {
            if (initialization == null || (initialization.Task.Status.IsCompleted() && !IsReady))
            {
                initialization = new UniTaskCompletionSource<bool>();
                ConnectAsync().Forget();
            }
            return initialization.Task.AttachExternalCancellation(cancellation);
        }

        async UniTaskVoid ConnectAsync()
        {
            try
            {
                if (store == null)
                {
                    store = UnityIAPServices.StoreController();
                    store.OnPurchasePending += OnPurchasePending;
                    store.OnPurchaseFailed += OnPurchaseFailed;
                    store.OnPurchaseDeferred += OnPurchaseDeferred;
                    store.OnProductsFetched += OnProductsFetched;
                    store.OnProductsFetchFailed += OnProductsFetchFailed;
                    store.OnStoreConnected += OnStoreConnected;
                    store.OnStoreDisconnected += OnStoreDisconnected;
                    store.OnPurchasesFetched += OnPurchasesFetched;
                    store.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
                }

                await store.Connect().AsUniTask();

                var products = new List<ProductDefinition>();
                foreach (CoinPack pack in catalog.packs) products.Add(new ProductDefinition(pack.productId, ProductType.Consumable));
                store.FetchProducts(products);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[IAP] Store unavailable: {exception.Message}");
                initialization.TrySetResult(false);
            }
        }

        void OnProductsFetched(List<Product> products)
        {
            IsReady = true;
            initialization.TrySetResult(true);
            store.FetchPurchases(); // re-delivers purchases interrupted before they were granted
        }

        void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogWarning($"[IAP] Products unavailable: {failure.FailureReason}");
            initialization.TrySetResult(false);
        }

        static void OnStoreConnected() => Debug.Log("[IAP] Store connected.");

        void OnStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            IsReady = false; // the next purchase reconnects first
            Debug.LogWarning($"[IAP] Store disconnected: {failure.Message}");
        }

        // Pending (unconfirmed) orders among these are re-delivered through OnPurchasePending and granted there.
        static void OnPurchasesFetched(Orders orders) { }

        static void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure) =>
            Debug.LogWarning($"[IAP] Could not restore purchases: {failure.Message}");

        public string PriceOf(string productId)
        {
            string price = store?.GetProductById(productId)?.metadata?.localizedPriceString;
            return string.IsNullOrEmpty(price) ? null : price;
        }

        public async UniTask<PurchaseResult> PurchaseAsync(string productId, CancellationToken cancellation)
        {
            if (!IsReady && !await InitializeAsync(cancellation))
                return PurchaseResult.Failure(productId, "The store is not available right now. Check your connection and try again.");
            if (inFlight.ContainsKey(productId))
                return PurchaseResult.Failure(productId, "This purchase is already in progress.");

            Product product = store.GetProductById(productId);
            if (product == null || !product.availableToPurchase)
                return PurchaseResult.Failure(productId, "This item is not available right now.");

            var completion = new UniTaskCompletionSource<PurchaseResult>();
            inFlight[productId] = completion;
            try
            {
                store.PurchaseProduct(product);
                return await completion.Task.AttachExternalCancellation(cancellation);
            }
            finally
            {
                inFlight.Remove(productId);
            }
        }

        void OnPurchasePending(PendingOrder order)
        {
            foreach (string productId in ProductIds(order))
            {
                // Grant first, then confirm: if the app dies in between, the order is re-delivered.
                Purchased?.Invoke(productId);
                Complete(productId, PurchaseResult.Success(productId));
            }
            store.ConfirmPurchase(order);
        }

        void OnPurchaseFailed(FailedOrder order)
        {
            Debug.Log($"[IAP] Purchase failed: {order.FailureReason} {order.Details}");
            foreach (string productId in ProductIds(order))
                Complete(productId, PurchaseResult.Failure(productId, Describe(order.FailureReason)));
        }

        void OnPurchaseDeferred(DeferredOrder order)
        {
            foreach (string productId in ProductIds(order))
                Complete(productId, PurchaseResult.Failure(productId, "Waiting for approval. Your coins arrive as soon as the purchase is approved."));
        }

        void Complete(string productId, PurchaseResult result)
        {
            if (inFlight.TryGetValue(productId, out UniTaskCompletionSource<PurchaseResult> completion)) completion.TrySetResult(result);
        }

        static IEnumerable<string> ProductIds(Order order)
        {
            foreach (CartItem item in order.CartOrdered.Items())
            {
                string id = item.Product?.definition?.id;
                if (!string.IsNullOrEmpty(id)) yield return id;
            }
        }

        static string Describe(PurchaseFailureReason reason) => reason switch
        {
            PurchaseFailureReason.UserCancelled => "The purchase was cancelled. You were not charged.",
            PurchaseFailureReason.PaymentDeclined => "The payment was declined. You were not charged.",
            PurchaseFailureReason.PurchasingUnavailable => "Purchases are disabled on this device.",
            PurchaseFailureReason.ProductUnavailable => "This item is not available right now.",
            PurchaseFailureReason.ExistingPurchasePending => "Another purchase is still being processed.",
            PurchaseFailureReason.StoreNotConnected => "Can't reach the store. Check your connection and try again.",
            _ => $"Something went wrong ({reason}). You were not charged.",
        };

        public void Dispose()
        {
            if (store == null) return;
            store.OnPurchasePending -= OnPurchasePending;
            store.OnPurchaseFailed -= OnPurchaseFailed;
            store.OnPurchaseDeferred -= OnPurchaseDeferred;
            store.OnProductsFetched -= OnProductsFetched;
            store.OnProductsFetchFailed -= OnProductsFetchFailed;
            store.OnStoreConnected -= OnStoreConnected;
            store.OnStoreDisconnected -= OnStoreDisconnected;
            store.OnPurchasesFetched -= OnPurchasesFetched;
            store.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
        }
    }
}
