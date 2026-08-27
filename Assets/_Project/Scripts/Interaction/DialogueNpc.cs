using UnityEngine;

/// <summary>
/// 퀘스트를 주지 않고 대사만 하는 NPC.
/// 대화 사실을 DialogueFlags에 기록하므로 TalkObjective의 대상이 될 수 있습니다.
/// </summary>
public class DialogueNpc : Interactable
{
    [SerializeField] private NpcData npcData;



    public override void Interact(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out PlayerProgress progress))
        {
            Debug.LogError($"{name}: 상호작용 대상에 PlayerProgress가 없습니다.");
            return;
        }

        if (npcData == null)
        {
            Debug.LogError($"{name}: NpcData가 할당되지 않았습니다.");
            return;
        }

        if (npcData.requiredQuest != null &&
            progress.Log.GetState(npcData.requiredQuest) != QuestState.Completed)
        {
            DialogueEvents.Request(npcData.displayName, npcData.lockedLine);
            return;
        }

        bool first = !progress.Flags.HasTalkedTo(npcData);
        progress.Flags.MarkTalked(npcData);

        string line = first || string.IsNullOrEmpty(npcData.repeatLine)
            ? npcData.firstLine
            : npcData.repeatLine;

        DialogueEvents.Request(npcData.displayName, line);
    }
}
