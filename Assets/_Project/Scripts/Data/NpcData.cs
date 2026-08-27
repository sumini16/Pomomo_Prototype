using UnityEngine;

/// <summary>대화 상대를 식별하는 데이터.</summary>
[CreateAssetMenu(fileName = "Npc_", menuName = "Game/Npc Data")]
public class NpcData : ScriptableObject
{
    [Tooltip("세이브 파일에 기록되는 고유 키. 한 번 정하면 바꾸지 않습니다.")]
    public string id;

    public string displayName;

    [Header("Dialogue")]
    [TextArea] public string firstLine;

    [Tooltip("두 번째 이후 대화. 비워두면 첫 대사를 반복합니다.")]
    [TextArea] public string repeatLine;


    [Header("Dialogue condition")]
    [Tooltip("완료되어야 첫 대사가 열리는 퀘스트")]
    public QuestData requiredQuest;

    [TextArea]
    public string lockedLine = "아직은 할 얘기가 없네.";

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
}