using UnityEngine;

/// <summary>특정 아이템을 일정 수량 '소지하고 있는가'로 판정합니다.</summary>
[CreateAssetMenu(fileName = "Obj_Collect_", menuName = "Game/Objective/Collect")]
public class CollectObjective : QuestObjective
{
    [SerializeField] private ItemData target;
    [SerializeField] private int required = 3;

    public override bool IsComplete(QuestContext ctx)
    {
        if (target == null || ctx?.Inventory == null) return false;

        // 경계 조건은 정확한 일치(==)가 아니라 범위(>=)로 판정합니다.
        return ctx.Inventory.GetCount(target) >= required;
    }

    public override string GetProgressText(QuestContext ctx)
    {
        if (target == null) return "(목표 미설정)";

        int count = ctx?.Inventory != null ? ctx.Inventory.GetCount(target) : 0;
        return $"{target.displayName}  {Mathf.Min(count, required)}/{required}";
    }

    public override void OnQuestCompleted(QuestContext ctx)
    {
        // 수집형만 아이템을 소모합니다. QuestGiver는 이 사실을 알지 못합니다.
        ctx?.Inventory?.Remove(target, required);
    }
}
