using UnityEngine;

/// <summary>
/// 퀘스트를 주고받는 NPC.
///
/// 여러 퀘스트를 순서대로 줄 수 있습니다. 배열 위에서부터 검사해
/// 아직 완료되지 않은 첫 번째 퀘스트를 진행하므로,
/// 앞 퀘스트를 끝내야 다음 퀘스트가 열립니다.
/// </summary>
public class QuestGiver : Interactable
{
    [Tooltip("이 NPC가 주는 퀘스트들. 위에서부터 순서대로 진행됩니다.")]
    [SerializeField] private QuestData[] quests;

    [SerializeField] private string speakerName = "마을 사람";

    [Tooltip("이 NPC 자체가 대화형 목표의 대상이 될 경우 지정합니다. 없으면 비워둡니다.")]
    [SerializeField] private NpcData npcData;

    [Tooltip("줄 퀘스트가 더 남아 있지 않을 때의 대사.")]
    [TextArea][SerializeField] private string idleText = "오늘은 별일 없네.";

    public override void Interact(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out PlayerProgress progress))
        {
            Debug.LogError($"{name}: 상호작용 대상에 PlayerProgress가 없습니다.");
            return;
        }

        QuestContext ctx = progress.Context;
        QuestLog log = progress.Log;

        progress.Flags.MarkTalked(npcData);

        QuestData quest = PickCurrentQuest(log);

        // 줄 퀘스트가 남아 있지 않음
        if (quest == null)
        {
            DialogueEvents.Request(speakerName, idleText);
            return;
        }

        // 선행 퀘스트가 아직 완료되지 않았다면 퀘스트를 열지 않습니다.
        if (!log.IsUnlocked(quest))
        {
            DialogueEvents.Request(speakerName, quest.lockedText);
            return;
        }

        switch (log.GetState(quest))
        {
            case QuestState.NotStarted:
                DialogueEvents.RequestChoice(
                    speakerName,
                    quest.acceptText,
                    onAccept: () =>
                    {
                        log.SetState(quest, QuestState.InProgress);
                        QuestEvents.Accepted(quest);
                    },
                    onDecline: () =>
                    {
                        DialogueEvents.Request(speakerName, quest.declineText);
                    });
                break;

            case QuestState.InProgress:
                if (quest.objective != null && quest.objective.IsComplete(ctx))
                {
                    quest.objective.OnQuestCompleted(ctx);

                    log.SetState(quest, QuestState.Completed);
                    QuestEvents.Completed(quest);
                    DialogueEvents.Request(speakerName, quest.completeText);
                }
                else
                {
                    string line = quest.objective != null
                        ? $"{quest.progressText} ({quest.objective.GetProgressText(ctx)})"
                        : quest.progressText;

                    DialogueEvents.Request(speakerName, line);
                }
                break;
        }
    }

    /// <summary>아직 완료되지 않은 첫 번째 퀘스트. 전부 끝냈으면 null.</summary>
    private QuestData PickCurrentQuest(QuestLog log)
    {
        if (quests == null) return null;

        foreach (QuestData q in quests)
        {
            if (q == null) continue;
            if (log.GetState(q) != QuestState.Completed) return q;
        }

        return null;
    }
}