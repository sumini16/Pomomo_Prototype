using System;
using UnityEngine;

public class ItemPickup : Interactable
{
    [Tooltip("세이브에 기록되는 고유 키. 같은 아이템이라도 배치마다 달라야 합니다. 예: berry_0")]
    [SerializeField] private string id;

    [SerializeField] private ItemData itemData;

    public static event Action<ItemData> OnAnyItemCollected;

    public string Id => id;

   
    public override string DisplayName =>
        itemData != null ? itemData.displayName : base.DisplayName;

    public override void Interact(GameObject interactor)
    {
        if (itemData == null)
        {
            Debug.LogError($"{name}: ItemData가 할당되지 않았습니다.", this);
            return;
        }

        if (!interactor.TryGetComponent(out PlayerProgress progress)) return;

        progress.Inventory.Add(itemData);
        progress.Pickups.MarkCollected(id);

        OnAnyItemCollected?.Invoke(itemData);

        gameObject.SetActive(false);
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            Debug.LogWarning($"[ItemPickup] {name}: Id가 비어 있어 저장되지 않습니다.", this);
    }
}