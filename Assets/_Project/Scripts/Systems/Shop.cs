using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>거래 결과. bool 대신 사유를 함께 돌려주어 UI가 안내 문구를 고를 수 있게 합니다.</summary>
public enum TradeResult
{
    Success,
    NotEnoughGold,
    NoItem,
    NotTradable,
    Invalid,
}

public class Shop : MonoBehaviour
{
    [SerializeField] private ItemData[] itemsForSale;

    public IReadOnlyList<ItemData> ItemsForSale => itemsForSale;

    public TradeResult TryBuy(PlayerProgress progress, ItemData item)
    {
        if (progress == null || item == null) return TradeResult.Invalid;
        if (!itemsForSale.Contains(item)) return TradeResult.NotTradable;
        if (item.buyPrice <= 0) return TradeResult.NotTradable;

        // 잔액 검사와 차감을 Wallet 안에 함께 두어 둘이 갈라지지 않게 합니다.
        if (!progress.Wallet.TrySpend(item.buyPrice)) return TradeResult.NotEnoughGold;

        progress.Inventory.Add(item);
        return TradeResult.Success;
    }

    public TradeResult TrySell(PlayerProgress progress, ItemData item)
    {
        if (progress == null || item == null) return TradeResult.Invalid;
        if (item.sellPrice <= 0) return TradeResult.NotTradable;
        if (!progress.Inventory.Remove(item)) return TradeResult.NoItem;

        progress.Wallet.Add(item.sellPrice);
        return TradeResult.Success;
    }
}