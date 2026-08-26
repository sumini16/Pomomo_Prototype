using TMPro;
using UnityEngine;

public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private GameObject trackerRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private PlayerInventory inventory;

    private QuestData activeQuest;

    private void Awake() => trackerRoot.SetActive(false);

    private void OnEnable()
    {
        QuestEvents.OnAccepted += HandleAccepted;
        QuestEvents.OnCompleted += HandleCompleted;
        inventory.OnInventoryChanged += RefreshObjective;
    }

    private void OnDisable()
    {
        QuestEvents.OnAccepted -= HandleAccepted;
        QuestEvents.OnCompleted -= HandleCompleted;
        inventory.OnInventoryChanged -= RefreshObjective;
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
        if (activeQuest == null) return;

        int count = inventory.GetCount(activeQuest.targetItem);
        objectiveText.text =
            $"{activeQuest.targetItem.displayName}  {count}/{activeQuest.requiredCount}";
    }
}