using UnityEngine;

public class QuestGiver : Interactable
{
    [SerializeField] private QuestData quest;
    [SerializeField] private QuestState questState;

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
                questState = QuestState.InProgress;
                Debug.Log($"[퀘스트 수락] {quest.acceptText}");
                break;

            case QuestState.InProgress:
                if (count >= quest.requiredCount)
                {
                    inventory.Remove(quest.targetItem, quest.requiredCount);
                    questState = QuestState.Completed;
                    Debug.Log($"[퀘스트 완료] {quest.completeText}");
                }
                else
                {
                    Debug.Log($"[{quest.progressText}] ({count}/{quest.requiredCount})");
                }
                break;

            case QuestState.Completed:
                Debug.Log($"[완료] {quest.completeText}");
                break;
        }
    }
}