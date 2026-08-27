using System.Linq;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private ItemData[] itemsForSale;

    public bool TryBuy(PlayerProgress progress, ItemData item)
    {
        if (progress == null || item == null) return false;
        if (!itemsForSale.Contains(item)) return false;
        if (item.buyPrice <= 0) return false;
        if (!progress.Wallet.TrySpend(item.buyPrice)) return false;

        progress.Inventory.Add(item);
        return true;
    }

    public bool TrySell(PlayerProgress progress, ItemData item)
    {
        if (progress == null || item == null) return false;
        if (item.sellPrice <= 0) return false;
        if (!progress.Inventory.Remove(item)) return false;

        progress.Wallet.Add(item.sellPrice);
        return true;
    }
}