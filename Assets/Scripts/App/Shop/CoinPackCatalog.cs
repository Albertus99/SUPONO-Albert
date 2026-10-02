using System;
using System.Collections.Generic;
using UnityEngine;

namespace Supono.App.Shop
{
    [Serializable]
    public sealed class CoinPack
    {
        [Tooltip("Store product id (consumable).")]
        public string productId;
        public string displayName;
        [Min(1)] public int coins;
        [Tooltip("Shown until the store reports its localized price.")]
        public string fallbackPrice;
        [Tooltip("Optional ribbon, e.g. \"Best Value\".")]
        public string badge;
        public Sprite icon;
    }

    /// <summary>Coin packs sold for real money through the store.</summary>
    [CreateAssetMenu(menuName = "Supono/Coin Pack Catalog", fileName = "CoinPackCatalog")]
    public sealed class CoinPackCatalog : ScriptableObject
    {
        public List<CoinPack> packs = new();

        public CoinPack Find(string productId) => packs.Find(p => p.productId == productId);
    }
}
