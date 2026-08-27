using TMPro;
using UnityEngine;

/// <summary>
/// 진행 중인 퀘스트를 표시합니다.
///
/// 데이터를 소유하지 않고 구독만 합니다.
/// 진행 문구를 직접 조립하지 않고 목표에게 물어보므로,
/// 목표 종류가 늘어나도 이 클래스는 수정되지 않습니다.
/// </summary>
public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private GameObject trackerRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private PlayerProgress progress;

    private QuestData activeQuest;

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

        progress.Inventory.OnInventoryChanged -= RefreshObjective;
        progress.Kills.OnKillsChanged -= RefreshObjective;
        progress.Flags.OnFlagsChanged -= RefreshObjective;
    }

    private void HandleAccepted(QuestData quest)
    {
        activeQuest = quest;
        titleText.text = quest.title;
        trackerRoot.SetActive(true);
        RefreshObjective();
    }

    private void HandleCompleted(QuestData quest)
    {
        if (quest != activeQuest) return;

        activeQuest = null;
        trackerRoot.SetActive(false);
    }

    private void RefreshObjective()
    {
        if (activeQuest == null || activeQuest.objective == null) return;
        if (progress == null) return;

        objectiveText.text = activeQuest.objective.GetProgressText(progress.Context);
    }
}
