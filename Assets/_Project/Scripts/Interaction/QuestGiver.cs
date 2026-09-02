using UnityEngine;

/// <summary>
/// 퀘스트를 주고받는 NPC.
/// 배열 순서대로 아직 완료되지 않은 첫 퀘스트를 진행합니다.
/// </summary>
public class QuestGiver : Interactable
{
    [Tooltip("이 NPC가 주는 퀘스트들. 위에서부터 순서대로 진행됩니다.")]
    [SerializeField] private QuestData[] quests;

    [SerializeField] private string speakerName = "마을 사람";

    [Tooltip("이 NPC 자체가 대화형 목표의 대상일 경우 지정합니다. 없으면 비워둡니다.")]
    [SerializeField] private NpcData npcData;

    [Tooltip("줄 퀘스트가 더 남아 있지 않을 때의 대사.")]
    [TextArea]
    [SerializeField] private string idleText = "오늘은 별일 없네.";

    public override void Interact(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out PlayerProgress progress))
        {
            Debug.LogError($"{name}: 상호작용 대상에 PlayerProgress가 없습니다.");
            return;
        }


        QuestContext ctx = progress.Context;
        QuestLog log = progress.Log;

        // 아직 소개받지 않은 인물은 대화 자체가 열리지 않습니다.
        // 여기서 막지 않으면, 순서를 건너뛰고 찾아온 것만으로 대화 목표가 달성됩니다.
        if (npcData != null && npcData.requiredQuest != null &&
            log.GetState(npcData.requiredQuest) == QuestState.NotStarted)
        {
            DialogueEvents.Request(speakerName, npcData.lockedLine);
            return;
        }


        progress.Flags.MarkTalked(npcData);

        QuestData quest = PickCurrentQuest(log);

        if (quest == null)
        {
            if (npcData != null) progress.Flags.MarkTalked(npcData);
            DialogueEvents.Request(speakerName, idleText);
            return;
        }

        if (!log.IsUnlocked(quest))
        {
            // 잠금 대사만 들려준 경우는 '만났다'로 치지 않습니다.
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

                    Debug.Log(
                        $"[QuestReward] 퀘스트 완료: {quest.title} | " +
                        $"설정 보상: {quest.rewardGold} | " +
                        $"지급 전 골드: {progress.Wallet.Gold}");

                    progress.Wallet.Add(quest.rewardGold);

                    Debug.Log($"[QuestReward] 지급 후 골드: {progress.Wallet.Gold}");

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

    /// <summary>
    /// 아직 완료되지 않은 첫 번째 퀘스트를 반환합니다.
    /// 전부 끝났다면 null을 반환합니다.
    /// </summary>
    private QuestData PickCurrentQuest(QuestLog log)
    {
        if (quests == null) return null;

        foreach (QuestData quest in quests)
        {
            if (quest == null) continue;

            if (log.GetState(quest) != QuestState.Completed)
                return quest;
        }

        return null;
    }
}