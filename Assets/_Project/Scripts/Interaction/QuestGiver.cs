using UnityEngine;

public class QuestGiver : Interactable
{
    [SerializeField] private QuestData quest;
    [SerializeField] private QuestState questState;
    [SerializeField] private string speakerName = "마을 사람";
    private void Awake()
    {
        if (quest == null)
            Debug.LogError($"{name}: QuestData가 할당되지 않았습니다.");
    }

    public override void Interact(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out PlayerInventory inventory))
            return;

        int count = inventory.GetCount(quest.targetItem);

        switch (questState)
        {
            case QuestState.NotStarted:
                DialogueEvents.RequestChoice(
                    speakerName,
                    quest.acceptText,
                    onAccept: () =>
                    {
                        questState = QuestState.InProgress;
                        QuestEvents.Accepted(quest);
                    },
                    onDecline: () =>
                    {
                        DialogueEvents.Request(speakerName, quest.declineText);
                    });
                break;

            case QuestState.InProgress:
                if (count >= quest.requiredCount)
                {
                    inventory.Remove(quest.targetItem, quest.requiredCount);
                    questState = QuestState.Completed;
                    QuestEvents.Completed(quest);
                    DialogueEvents.Request(speakerName, quest.completeText);      // ← 완료
                }
                else
                {
                    DialogueEvents.Request(speakerName,
                        $"{quest.progressText} ({count}/{quest.requiredCount})"); // ← 진행 중
                }
                break;

            case QuestState.Completed:
                DialogueEvents.Request(speakerName, quest.completeText);          // ← 완료 후
                break;
        }
    }
}