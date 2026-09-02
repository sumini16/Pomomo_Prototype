using System;
using UnityEngine;

/// <summary>
/// 세이브 파일에 기록된 id를 다시 에셋으로 되돌리기 위한 조회표입니다.
///
/// JsonUtility는 ScriptableObject 참조를 저장하지 못하므로 파일에는 문자열 id만 남습니다.
/// 복원 시점에 그 문자열을 실제 에셋으로 바꿔줄 곳이 한 군데는 있어야 해서 만들었습니다.
/// </summary>
[CreateAssetMenu(fileName = "GameDatabase", menuName = "Game/Game Database")]
public class GameDatabase : ScriptableObject
{
    [SerializeField] private ItemData[] items;
    [SerializeField] private QuestData[] quests;
    [SerializeField] private EnemyData[] enemies;
    [SerializeField] private NpcData[] npcs;

    public ItemData GetItem(string id) => Find(items, id, x => x.id);
    public QuestData GetQuest(string id) => Find(quests, id, x => x.id);
    public EnemyData GetEnemy(string id) => Find(enemies, id, x => x.id);
    public NpcData GetNpc(string id) => Find(npcs, id, x => x.id);

    private static T Find<T>(T[] source, string id, Func<T, string> idOf) where T : ScriptableObject
    {
        if (source == null || string.IsNullOrEmpty(id)) return null;

        foreach (T entry in source)
        {
            if (entry != null && idOf(entry) == id) return entry;
        }

        return null;
    }

    private void OnValidate()
    {
        Validate(items, x => x.id, "Items");
        Validate(quests, x => x.id, "Quests");
        Validate(enemies, x => x.id, "Enemies");
        Validate(npcs, x => x.id, "Npcs");
    }

    /// <summary>
    /// 빈 id와 중복 id는 복원 시 예외 없이 조용히 잘못된 결과를 냅니다.
    /// 실행 중에 알아채기 어려우므로 에디터에서 미리 걸러냅니다.
    /// </summary>
    private void Validate<T>(T[] source, Func<T, string> idOf, string label) where T : ScriptableObject
    {
        if (source == null) return;

        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == null) continue;

            string id = idOf(source[i]);

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError($"[GameDatabase] {label}: '{source[i].name}'의 id가 비어 있습니다.", source[i]);
                continue;
            }

            for (int j = i + 1; j < source.Length; j++)
            {
                if (source[j] == null) continue;

                if (idOf(source[j]) == id)
                    Debug.LogError($"[GameDatabase] {label}: id 중복 '{id}'  {source[i].name}, {source[j].name}", this);
            }
        }
    }
}