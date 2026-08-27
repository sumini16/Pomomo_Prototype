using UnityEngine;

/// <summary>대화 상대를 식별하는 데이터.</summary>
[CreateAssetMenu(fileName = "Npc_", menuName = "Game/Npc Data")]
public class NpcData : ScriptableObject
{
    [Tooltip("세이브 파일에 기록되는 고유 키. 한 번 정하면 바꾸지 않습니다.")]
    public string id;

    public string displayName;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
}
