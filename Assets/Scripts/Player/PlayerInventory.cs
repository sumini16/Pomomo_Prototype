using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private readonly Dictionary<ItemData, int> items = new();

    public event Action OnInventoryChanged;
    public IReadOnlyDictionary<ItemData, int> Items => items;

    public void Add(ItemData item, int amount = 1)
    {
        if (items.ContainsKey(item))
            items[item] += amount;
        else
            items[item] = amount;

        OnInventoryChanged?.Invoke();
    }

    public bool Remove(ItemData item, int amount = 1)
    {
        if (GetCount(item) < amount) return false;

        items[item] -= amount;

        if (items[item] <= 0)
            items.Remove(item);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetCount(ItemData item)
    {
        return items.TryGetValue(item, out int count) ? count : 0;
    }
}