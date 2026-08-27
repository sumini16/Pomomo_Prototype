using UnityEngine;

[CreateAssetMenu(fileName = "Quest_", menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    public string title;

    [Tooltip("완료 조건. 수집·처치·대화 중 하나를 에셋으로 만들어 할당합니다.")]
    public QuestObjective objective;

    [Tooltip("이 퀘스트가 열리기 위해 먼저 완료되어야 하는 퀘스트. 없으면 비워둡니다.")]
    public QuestData prerequisite;

    [Header("대사")]
    [TextArea] public string acceptText;
    [TextArea] public string progressText;
    [TextArea] public string completeText;
    [TextArea] public string declineText;

    [Tooltip("선행 퀘스트가 아직 완료되지 않았을 때의 대사.")]
    [TextArea] public string lockedText = "지금은 할 얘기가 없네.";
}
