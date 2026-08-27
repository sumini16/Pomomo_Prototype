using System;
using UnityEngine;

public class ItemPickup : Interactable
{
    [SerializeField] private ItemData itemData;
    public static event Action<ItemData> OnAnyItemCollected;
    public override void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out PlayerInventory inventory))
            inventory.Add(itemData);

        OnAnyItemCollected?.Invoke(itemData);
        gameObject.SetActive(false);
    }

    
}
