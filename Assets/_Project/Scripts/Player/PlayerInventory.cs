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

    /// <summary>
    /// 저장된 목록으로 통째로 되돌립니다.
    /// 현재 내용에 더하는 것이 아니라 교체하므로, 불러오기 전에 얻은 아이템은 사라집니다.
    /// </summary>
    public void Restore(IEnumerable<KeyValuePair<ItemData, int>> saved)
    {
        items.Clear();

        if (saved != null)
        {
            foreach (KeyValuePair<ItemData, int> pair in saved)
            {
                if (pair.Key == null || pair.Value <= 0) continue;
                items[pair.Key] = pair.Value;
            }
        }

        OnInventoryChanged?.Invoke();
    }
}