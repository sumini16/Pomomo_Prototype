using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 퀘스트의 진행 상태를 한 곳에서 소유합니다.
///
/// 상태를 QuestGiver가 각자 들고 있으면, 퀘스트 체인을 만들 때
/// B의 NPC가 A의 QuestGiver를 직접 참조해야 합니다(NPC끼리 서로 아는 구조).
/// 여기로 모으면 QuestGiver는 서로를 모른 채 선행 조건만 물어보면 됩니다.
///
/// 설정 데이터(QuestData)는 ScriptableObject에, 런타임 상태는 여기에 둡니다.
/// </summary>
public class QuestLog : MonoBehaviour
{
    private readonly Dictionary<QuestData, QuestState> states = new();

    public event Action OnQuestStateChanged;
    public IReadOnlyDictionary<QuestData, QuestState> States => states;

    public QuestState GetState(QuestData quest)
    {
        if (quest == null) return QuestState.NotStarted;
        return states.TryGetValue(quest, out QuestState state) ? state : QuestState.NotStarted;
    }

    public void SetState(QuestData quest, QuestState state)
    {
        if (quest == null) return;
        if (GetState(quest) == state) return;

        states[quest] = state;
        OnQuestStateChanged?.Invoke();
    }

    /// <summary>선행 퀘스트가 없거나, 있다면 그것이 완료되었는가.</summary>
    public bool IsUnlocked(QuestData quest)
    {
        if (quest == null) return false;
        if (quest.prerequisite == null) return true;

        return GetState(quest.prerequisite) == QuestState.Completed;
    }

    /// <summary>세이브 불러오기용.</summary>
    public void Restore(IEnumerable<KeyValuePair<QuestData, QuestState>> saved)
    {
        states.Clear();
        foreach (var pair in saved)
        {
            if (pair.Key != null) states[pair.Key] = pair.Value;
        }
        OnQuestStateChanged?.Invoke();
    }
}
