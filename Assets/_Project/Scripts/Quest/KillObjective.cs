using UnityEngine;

/// <summary>특정 종류의 적을 일정 횟수 처치했는가로 판정합니다.</summary>
[CreateAssetMenu(fileName = "Obj_Kill_", menuName = "Game/Objective/Kill")]
public class KillObjective : QuestObjective
{
    [SerializeField] private EnemyData target;
    [SerializeField] private int required = 3;

    public override bool IsComplete(QuestContext ctx)
    {
        if (target == null || ctx?.Kills == null) return false;
        return ctx.Kills.GetCount(target) >= required;
    }

    public override string GetProgressText(QuestContext ctx)
    {
        if (target == null) return "(목표 미설정)";

        int count = ctx?.Kills != null ? ctx.Kills.GetCount(target) : 0;
        return $"{target.displayName} 처치  {Mathf.Min(count, required)}/{required}";
    }
}
