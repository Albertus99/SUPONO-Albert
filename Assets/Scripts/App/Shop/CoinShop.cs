using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Supono.App.Analytics;
using Supono.App.Progress;
using VContainer.Unity;

namespace Supono.App.Shop
{
    /// <summary>
    /// Sells coin packs: grants coins for every completed store order (also ones re-delivered after a
    /// restart) and reports purchase analytics. Starts connecting to the store at boot.
    /// </summary>
    public sealed class CoinShop : IStartable, IDisposable
    {
        readonly IIapStore store;
        readonly CoinPackCatalog catalog;
        readonly ProgressService progress;
        readonly IAnalytics analytics;
        readonly CancellationTokenSource lifetime = new();

        public CoinShop(IIapStore store, CoinPackCatalog catalog, ProgressService progress, IAnalytics analytics)
        {
            this.store = store;
            this.catalog = catalog;
            this.progress = progress;
            this.analytics = analytics;
        }

        public CoinPackCatalog Catalog => catalog;

        public void Start()
        {
            store.Purchased += Grant;
            store.InitializeAsync(lifetime.Token).SuppressCancellationThrow().Forget();
        }

        public UniTask<bool> InitializeAsync(CancellationToken cancellation) => store.InitializeAsync(cancellation);

        public string PriceOf(CoinPack pack) => store.PriceOf(pack.productId) ?? pack.fallbackPrice;

        public async UniTask<PurchaseResult> BuyAsync(CoinPack pack, CancellationToken cancellation)
        {
            analytics.Log(new PurchaseClickEvent(pack.productId, PriceOf(pack)));
            PurchaseResult result = await store.PurchaseAsync(pack.productId, cancellation);
            analytics.Log(new PurchaseResultEvent(pack.productId, result.Succeeded, result.Message));
            return result;
        }

        void Grant(string productId)
        {
            CoinPack pack = catalog.Find(productId);
            if (pack != null) progress.AddCoins(pack.coins);
        }

        public void Dispose()
        {
            store.Purchased -= Grant;
            lifetime.Cancel();
            lifetime.Dispose();
        }
    }
}
