using UnityEngine;

/// <summary>
/// 퀘스트를 주지 않고 대사만 하는 NPC.
/// 대화 사실을 DialogueFlags에 기록하므로 TalkObjective의 대상이 될 수 있습니다.
/// </summary>
public class DialogueNpc : Interactable
{
    [SerializeField] private NpcData npcData;
    [SerializeField] private string speakerName = "아이";

    [TextArea] [SerializeField] private string firstLine = "...누구세요?";

    [Tooltip("두 번째 이후 대화에서 사용할 대사. 비워두면 첫 대사를 계속 사용합니다.")]
    [TextArea] [SerializeField] private string repeatLine;

    public override void Interact(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out PlayerProgress progress))
        {
            Debug.LogError($"{name}: 상호작용 대상에 PlayerProgress가 없습니다.");
            return;
        }

        bool first = !progress.Flags.HasTalkedTo(npcData);

        progress.Flags.MarkTalked(npcData);

        string line = first || string.IsNullOrEmpty(repeatLine) ? firstLine : repeatLine;
        DialogueEvents.Request(speakerName, line);
    }
}
