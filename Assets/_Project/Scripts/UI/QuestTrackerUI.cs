using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 진행 중인 퀘스트를 표시합니다.
///
/// 데이터를 소유하지 않고 구독만 합니다.
/// 진행 문구를 직접 조립하지 않고 목표에게 물어보므로,
/// 목표 종류가 늘어나도 이 클래스는 수정되지 않습니다.
///
/// 표시 대상은 QuestLog의 현재 상태에서 가져옵니다.
/// 수락 이벤트에만 의존하면 세이브 불러오기처럼 사건 없이 상태만 바뀌는 경우를 놓칩니다(2.13과 같은 이유).
/// 다만 여러 퀘스트가 동시에 진행될 수 있으므로, 방금 수락한 것을 우선 표시합니다.
/// </summary>
public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private GameObject trackerRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private PlayerProgress progress;

    private QuestData activeQuest;

    /// <summary>가장 최근에 수락한 퀘스트. 표시 우선순위를 정하는 힌트로만 씁니다.</summary>
    private QuestData lastAccepted;

    private void Awake()
    {
        trackerRoot.SetActive(false);

        if (progress == null)
            Debug.LogError($"{name}: PlayerProgress가 할당되지 않았습니다.");
    }

    private void OnEnable()
    {
        QuestEvents.OnAccepted += HandleAccepted;
        QuestEvents.OnCompleted += HandleCompleted;

        if (progress == null) return;

        // 퀘스트 상태가 바뀌면 표시 대상을 다시 정합니다. 불러오기도 여기로 들어옵니다.
        progress.Log.OnQuestStateChanged += Refresh;

        // 세 저장소 중 무엇이 바뀌든 진행 문구를 다시 물어봅니다.
        progress.Inventory.OnInventoryChanged += RefreshObjective;
        progress.Kills.OnKillsChanged += RefreshObjective;
        progress.Flags.OnFlagsChanged += RefreshObjective;
    }

    private void OnDisable()
    {
        QuestEvents.OnAccepted -= HandleAccepted;
        QuestEvents.OnCompleted -= HandleCompleted;

        if (progress == null) return;

        progress.Log.OnQuestStateChanged -= Refresh;

        progress.Inventory.OnInventoryChanged -= RefreshObjective;
        progress.Kills.OnKillsChanged -= RefreshObjective;
        progress.Flags.OnFlagsChanged -= RefreshObjective;
    }

    private void Start()
    {
        // 이미 진행 중인 퀘스트가 있는 상태로 시작할 수 있습니다.
        Refresh();
    }

    private void HandleAccepted(QuestData quest)
    {
        lastAccepted = quest;
        Refresh();
    }

    private void HandleCompleted(QuestData quest)
    {
        if (lastAccepted == quest) lastAccepted = null;
        Refresh();
    }

    /// <summary>표시할 퀘스트를 상태에서 다시 고르고 화면을 갱신합니다.</summary>
    private void Refresh()
    {
        if (progress == null) return;

        activeQuest = PickQuestToShow();

        if (activeQuest == null)
        {
            trackerRoot.SetActive(false);
            return;
        }

        trackerRoot.SetActive(true);
        titleText.text = activeQuest.title;
        RefreshObjective();
    }

    /// <summary>
    /// 방금 수락한 퀘스트를 우선합니다. NPC가 둘이라 두 퀘스트를 동시에 받을 수 있는데,
    /// Dictionary 순회 순서는 보장되지 않아 그대로 두면 표시 대상이 들쭉날쭉해집니다.
    /// 불러오기 직후처럼 힌트가 없을 때만 진행 중인 것 중 하나를 고릅니다.
    /// </summary>
    private QuestData PickQuestToShow()
    {
        if (lastAccepted != null && progress.Log.GetState(lastAccepted) == QuestState.InProgress)
            return lastAccepted;

        foreach (KeyValuePair<QuestData, QuestState> pair in progress.Log.States)
        {
            if (pair.Key != null && pair.Value == QuestState.InProgress)
                return pair.Key;
        }

        return null;
    }

    private void RefreshObjective()
    {
        if (activeQuest == null || activeQuest.objective == null) return;
        if (progress == null) return;

        objectiveText.text = activeQuest.objective.GetProgressText(progress.Context);
    }
}