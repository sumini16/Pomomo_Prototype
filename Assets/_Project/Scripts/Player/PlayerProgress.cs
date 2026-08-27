using UnityEngine;

/// <summary>
/// 플레이어의 진행 상태 저장소들을 한 곳에서 묶어 QuestContext로 제공합니다.
///
/// 여기 모인 네 컴포넌트(+ 이후 추가될 골드)가 그대로 세이브 대상 전체입니다.
///
/// 오브젝트 간 Awake 호출 순서는 보장되지 않으므로, UI가 OnEnable에서
/// 이 컴포넌트를 참조할 때 아직 Awake가 돌지 않았을 수 있습니다.
/// 실행 순서를 강제하는 대신 접근 시점에 스스로 초기화하도록 했습니다.
/// </summary>
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(KillTracker))]
[RequireComponent(typeof(DialogueFlags))]
[RequireComponent(typeof(QuestLog))]
[RequireComponent(typeof(Wallet))]
public class PlayerProgress : MonoBehaviour
{
    private PlayerInventory inventory;
    private KillTracker kills;
    private DialogueFlags flags;
    private QuestLog log;
    private QuestContext context;
    private Wallet wallet;
    public PlayerInventory Inventory { get { EnsureInitialized(); return inventory; } }
    public KillTracker Kills { get { EnsureInitialized(); return kills; } }
    public DialogueFlags Flags { get { EnsureInitialized(); return flags; } }
    public QuestLog Log { get { EnsureInitialized(); return log; } }
    public QuestContext Context { get { EnsureInitialized(); return context; } }

    public Wallet Wallet { get { EnsureInitialized(); return wallet; } }

    private void Awake() => EnsureInitialized();

    private void EnsureInitialized()
    {
        if (context != null) return;

        inventory = GetComponent<PlayerInventory>();
        kills = GetComponent<KillTracker>();
        flags = GetComponent<DialogueFlags>();
        log = GetComponent<QuestLog>();
        wallet = GetComponent<Wallet>();

        context = new QuestContext(inventory, kills, flags, log);
    }
}
