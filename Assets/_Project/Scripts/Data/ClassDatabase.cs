using UnityEngine;

/// <summary>
/// 직업 목록. 선택 화면이 순회하고, 세이브 복원이 id로 조회합니다.
/// 씬마다 직업 목록을 따로 들고 있으면 어긋나므로 에셋 하나로 모읍니다.
/// </summary>
[CreateAssetMenu(fileName = "ClassDatabase", menuName = "Game/Class Database")]
public class ClassDatabase : ScriptableObject
{
    [SerializeField] private ClassData[] classes;

    public ClassData[] All => classes;

    public ClassData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (ClassData data in classes)
        {
            if (data != null && data.id == id) return data;
        }
        return null;
    }

    private void OnValidate()
    {
        if (classes == null) return;

        for (int i = 0; i < classes.Length; i++)
        {
            if (classes[i] == null) continue;

            // 빈 id는 세이브 복원 시 아무것도 못 찾습니다.
            if (string.IsNullOrWhiteSpace(classes[i].id))
            {
                Debug.LogError($"[ClassDatabase] '{classes[i].name}'의 id가 비어 있습니다.", classes[i]);
                continue;
            }

            // 중복 id는 조회 결과가 순서에 좌우되어 조용히 잘못된 직업이 복원됩니다.
            for (int j = i + 1; j < classes.Length; j++)
            {
                if (classes[j] == null) continue;
                if (string.IsNullOrWhiteSpace(classes[j].id)) continue;

                if (classes[i].id == classes[j].id)
                    Debug.LogError($"[ClassDatabase] id 중복: '{classes[i].id}'  {classes[i].name}, {classes[j].name}", this);
            }
        }
    }
}