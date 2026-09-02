using System;
using UnityEngine;

/// <summary>사용 시도의 결과. 실패 사유가 여러 가지라 bool로는 부족합니다.</summary>
public enum UseResult
{
    Success,
    NotUsable,
    NoItem,
    AlreadyFull
}

/// <summary>
/// 아이템을 사용해 효과를 적용합니다.
///
/// 판정은 여기서 하고, 결과를 문구로 바꾸는 일은 UI가 맡습니다(Shop과 같은 구분).
/// 회복량은 ItemData가 들고 있으므로, 새 소모품을 추가할 때 코드는 바뀌지 않습니다.
/// </summary>
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(Health))]
public class ItemUser : MonoBehaviour
{
    private PlayerInventory inventory;
    private Health health;

    /// <summary>사용 결과를 알립니다. UI가 구독해 문구를 띄웁니다.</summary>
    public event Action<ItemData, UseResult> OnItemUsed;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        health = GetComponent<Health>();
    }

    public UseResult TryUse(ItemData item)
    {
        UseResult result = Evaluate(item);

        if (result == UseResult.Success)
        {
            // 소모가 먼저입니다. 회복이 인벤토리 변경 이벤트를 타고 UI를 다시 그리는데,
            // 그 시점에 아직 아이템이 남아 있으면 한 프레임 동안 개수가 어긋나 보입니다.
            inventory.Remove(item, 1);
            health.Heal(item.healAmount);
        }

        OnItemUsed?.Invoke(item, result);
        return result;
    }

    private UseResult Evaluate(ItemData item)
    {
        if (item == null || item.healAmount <= 0) return UseResult.NotUsable;
        if (inventory.GetCount(item) <= 0) return UseResult.NoItem;
        if (health.Current >= health.Max) return UseResult.AlreadyFull;

        return UseResult.Success;
    }
}