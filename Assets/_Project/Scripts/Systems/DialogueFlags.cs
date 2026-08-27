using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "누구와 대화한 적이 있는가"를 보관합니다.
/// TalkObjective가 물어볼 수 있는 상태를 만들어 주는 저장소입니다.
/// </summary>
public class DialogueFlags : MonoBehaviour
{
    private readonly HashSet<NpcData> talkedTo = new();

    public event Action OnFlagsChanged;
    public IReadOnlyCollection<NpcData> TalkedTo => talkedTo;

    public void MarkTalked(NpcData npc)
    {
        if (npc == null) return;

        // 이미 기록돼 있으면 이벤트를 다시 쏘지 않습니다.
        if (!talkedTo.Add(npc)) return;

        OnFlagsChanged?.Invoke();
    }

    public bool HasTalkedTo(NpcData npc)
    {
        return npc != null && talkedTo.Contains(npc);
    }

    /// <summary>세이브 불러오기용.</summary>
    public void Restore(IEnumerable<NpcData> saved)
    {
        talkedTo.Clear();
        foreach (var npc in saved)
        {
            if (npc != null) talkedTo.Add(npc);
        }
        OnFlagsChanged?.Invoke();
    }
}
