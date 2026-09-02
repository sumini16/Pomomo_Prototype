using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 저장 대상을 모으고 되돌리는 곳입니다.
///
/// 각 시스템이 스스로 파일을 읽고 쓰면 저장 시점이 흩어져 순서를 통제할 수 없습니다.
/// 반대로 이 클래스가 각 시스템의 내부 자료구조를 직접 고치면 결합도가 올라갑니다.
/// 그래서 각 시스템은 자기 상태를 되돌리는 메서드(Restore)만 갖고,
/// 이 클래스는 그것을 모아 SaveData로 조립하는 역할만 맡습니다.
/// </summary>
public class SaveCoordinator : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameDatabase database;

    [Header("자동 저장")]
    [Tooltip("퀘스트 상태가 바뀔 때마다 저장합니다.")]
    [SerializeField] private bool autoSaveOnQuestChange = true;

    [Header("불러오기")]
    [Tooltip("씬이 시작될 때 저장 파일이 있으면 자동으로 복원합니다.")]
    [SerializeField] private bool loadOnStart;

    private CharacterController playerController;
    private bool restoring;

    private void Awake()
    {
        if (progress == null) progress = FindFirstObjectByType<PlayerProgress>();

        if (progress == null)
        {
            Debug.LogError("[SaveCoordinator] PlayerProgress를 찾지 못했습니다.", this);
            return;
        }

        if (playerTransform == null) playerTransform = progress.transform;
        playerController = playerTransform.GetComponent<CharacterController>();

        if (database == null)
            Debug.LogError("[SaveCoordinator] GameDatabase가 비어 있습니다. 불러오기가 동작하지 않습니다.", this);
    }

    private void OnEnable()
    {
        if (progress == null || !autoSaveOnQuestChange) return;
        progress.Log.OnQuestStateChanged += HandleQuestStateChanged;
    }

    private void OnDisable()
    {
        if (progress == null || !autoSaveOnQuestChange) return;
        progress.Log.OnQuestStateChanged -= HandleQuestStateChanged;
    }

    private void Start()
    {
        if (loadOnStart && SaveSystem.HasSave) Load();
    }

    private void HandleQuestStateChanged()
    {
        // 복원 중에는 상태가 연달아 바뀌며 이벤트가 여러 번 발생합니다.
        // 그때마다 저장하면 방금 불러온 내용을 다시 쓰는 낭비가 생깁니다.
        if (restoring) return;

        Save();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f5Key.wasPressedThisFrame) Save();
        if (Keyboard.current.f9Key.wasPressedThisFrame) Load();
    }

    // ────────────────────────────── 저장

    public void Save()
    {
        if (progress == null) return;
        SaveSystem.Save(Capture());
    }

    private SaveData Capture()
    {
        SaveData data = new SaveData();

        ClassData currentClass = GameManager.Instance != null ? GameManager.Instance.SelectedClass : null;
        data.classId = currentClass != null ? currentClass.id : string.Empty;

        data.gold = progress.Wallet.Gold;

        if (playerTransform != null)
        {
            Vector3 position = playerTransform.position;
            data.posX = position.x;
            data.posY = position.y;
            data.posZ = position.z;
            data.rotY = playerTransform.eulerAngles.y;
        }

        foreach (KeyValuePair<ItemData, int> pair in progress.Inventory.Items)
        {
            if (pair.Key == null) continue;
            data.inventory.Add(new SaveData.CountEntry { id = pair.Key.id, count = pair.Value });
        }

        foreach (KeyValuePair<EnemyData, int> pair in progress.Kills.Kills)
        {
            if (pair.Key == null) continue;
            data.kills.Add(new SaveData.CountEntry { id = pair.Key.id, count = pair.Value });
        }

        foreach (KeyValuePair<QuestData, QuestState> pair in progress.Log.States)
        {
            if (pair.Key == null) continue;
            data.quests.Add(new SaveData.QuestEntry { id = pair.Key.id, state = pair.Value.ToString() });
        }

        foreach (NpcData npc in progress.Flags.TalkedTo)
        {
            if (npc == null) continue;
            data.talkedNpcIds.Add(npc.id);
        }

        return data;
    }

    // ────────────────────────────── 불러오기

    public void Load()
    {
        SaveData data = SaveSystem.Load();
        if (data == null) return;

        if (database == null)
        {
            Debug.LogError("[SaveCoordinator] GameDatabase가 없어 복원할 수 없습니다.", this);
            return;
        }

        restoring = true;
        Restore(data);
        restoring = false;

        Debug.Log("[SaveCoordinator] 불러오기 완료");
    }

    private void Restore(SaveData data)
    {
        if (GameManager.Instance != null && !string.IsNullOrEmpty(data.classId))
            GameManager.Instance.SelectClassById(data.classId);

        progress.Wallet.Restore(data.gold);

        RestorePosition(data);

        progress.Inventory.Restore(ToPairs(data.inventory, database.GetItem));
        progress.Kills.Restore(ToPairs(data.kills, database.GetEnemy));
        progress.Log.Restore(ToQuestPairs(data.quests));
        progress.Flags.Restore(ToNpcs(data.talkedNpcIds));
    }

    private void RestorePosition(SaveData data)
    {
        if (playerTransform == null) return;

        // CharacterController는 자기 위치를 스스로 관리하므로,
        // 켜진 상태에서 transform을 옮기면 다음 프레임에 되돌려집니다.
        bool wasEnabled = playerController != null && playerController.enabled;
        if (wasEnabled) playerController.enabled = false;

        playerTransform.position = new Vector3(data.posX, data.posY, data.posZ);
        playerTransform.rotation = Quaternion.Euler(0f, data.rotY, 0f);

        if (wasEnabled) playerController.enabled = true;
    }

    // ────────────────────────────── id → 에셋

    private static List<KeyValuePair<T, int>> ToPairs<T>(
        List<SaveData.CountEntry> entries, Func<string, T> resolve) where T : ScriptableObject
    {
        List<KeyValuePair<T, int>> result = new List<KeyValuePair<T, int>>();

        foreach (SaveData.CountEntry entry in entries)
        {
            T asset = resolve(entry.id);

            // 에셋을 못 찾으면 그 항목만 건너뜁니다. 나머지는 정상 복원됩니다.
            if (asset == null)
            {
                Debug.LogWarning($"[SaveCoordinator] id '{entry.id}'에 해당하는 에셋을 찾지 못했습니다. GameDatabase 등록을 확인하세요.");
                continue;
            }

            result.Add(new KeyValuePair<T, int>(asset, entry.count));
        }

        return result;
    }

    private List<KeyValuePair<QuestData, QuestState>> ToQuestPairs(List<SaveData.QuestEntry> entries)
    {
        List<KeyValuePair<QuestData, QuestState>> result = new List<KeyValuePair<QuestData, QuestState>>();

        foreach (SaveData.QuestEntry entry in entries)
        {
            QuestData quest = database.GetQuest(entry.id);

            if (quest == null)
            {
                Debug.LogWarning($"[SaveCoordinator] 퀘스트 id '{entry.id}'를 찾지 못했습니다.");
                continue;
            }

            if (!Enum.TryParse(entry.state, out QuestState state))
            {
                Debug.LogWarning($"[SaveCoordinator] 알 수 없는 퀘스트 상태 '{entry.state}'  건너뜁니다.");
                continue;
            }

            result.Add(new KeyValuePair<QuestData, QuestState>(quest, state));
        }

        return result;
    }

    private List<NpcData> ToNpcs(List<string> ids)
    {
        List<NpcData> result = new List<NpcData>();

        foreach (string id in ids)
        {
            NpcData npc = database.GetNpc(id);

            if (npc == null)
            {
                Debug.LogWarning($"[SaveCoordinator] NPC id '{id}'를 찾지 못했습니다.");
                continue;
            }

            result.Add(npc);
        }

        return result;
    }
}