using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이미 획득한 월드 아이템을 기록합니다.
///
/// 인벤토리는 '무엇을 가지고 있는가'를, 이쪽은 '맵의 어느 것을 이미 주웠는가'를 담당합니다.
/// 둘을 함께 저장하지 않으면 불러왔을 때 인벤토리와 맵에 같은 아이템이 동시에 존재합니다.
/// </summary>
public class PickupTracker : MonoBehaviour
{
    private readonly HashSet<string> collected = new HashSet<string>();

    public event Action OnCollectedChanged;
    public IReadOnlyCollection<string> Collected => collected;

    public bool IsCollected(string id) => !string.IsNullOrEmpty(id) && collected.Contains(id);

    public void MarkCollected(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!collected.Add(id)) return;

        OnCollectedChanged?.Invoke();
    }

    /// <summary>세이브 불러오기용. 기록을 되돌리고 해당 오브젝트를 씬에서 감춥니다.</summary>
    public void Restore(IEnumerable<string> saved)
    {
        collected.Clear();

        if (saved != null)
        {
            foreach (string id in saved)
            {
                if (!string.IsNullOrEmpty(id)) collected.Add(id);
            }
        }

        ApplyToScene();
        OnCollectedChanged?.Invoke();
    }

    /// <summary>
    /// 씬을 훑어 기록과 상태를 맞춥니다.
    /// 각 픽업이 스스로 트래커를 찾게 하면 참조가 흩어지므로, 훑는 일은 여기에 모았습니다.
    /// </summary>
    private void ApplyToScene()
    {
        ItemPickup[] pickups = FindObjectsByType<ItemPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (ItemPickup pickup in pickups)
        {
            if (pickup == null) continue;
            pickup.gameObject.SetActive(!IsCollected(pickup.Id));
        }
    }
}