using UnityEngine;

/// <summary>특정 NPC와 대화한 적이 있는가로 판정합니다.</summary>
[CreateAssetMenu(fileName = "Obj_Talk_", menuName = "Game/Objective/Talk")]
public class TalkObjective : QuestObjective
{
    [SerializeField] private NpcData target;

    public override bool IsComplete(QuestContext ctx)
    {
        if (target == null || ctx?.Flags == null) return false;
        return ctx.Flags.HasTalkedTo(target);
    }

    public override string GetProgressText(QuestContext ctx)
    {
        if (target == null) return "(목표 미설정)";

        bool done = ctx?.Flags != null && ctx.Flags.HasTalkedTo(target);
        return $"{target.displayName} 만나기  {(done ? "완료" : "미완료")}";
    }
}
