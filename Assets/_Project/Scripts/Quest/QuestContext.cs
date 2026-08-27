/// <summary>
/// 퀘스트 목표가 완료 여부를 물어보는 상태 저장소들의 묶음.
///
/// 목표에 GameObject를 넘기고 안에서 GetComponent를 하는 방법도 있지만,
/// 그러면 각 목표가 무엇에 의존하는지가 코드 안쪽에 숨습니다.
/// 이렇게 묶으면 의존 대상이 시그니처에 드러나고,
/// 목표 종류가 늘어나도 IsComplete의 시그니처는 바뀌지 않습니다.
/// </summary>
public class QuestContext
{
    public PlayerInventory Inventory { get; }
    public KillTracker Kills { get; }
    public DialogueFlags Flags { get; }
    public QuestLog Log { get; }

    public QuestContext(PlayerInventory inventory, KillTracker kills,
                        DialogueFlags flags, QuestLog log)
    {
        Inventory = inventory;
        Kills = kills;
        Flags = flags;
        Log = log;
    }
}
