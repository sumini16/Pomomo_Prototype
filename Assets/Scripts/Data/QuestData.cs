using UnityEngine;

[CreateAssetMenu(fileName = "Quest_", menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    public string title;
    [TextArea] public string acceptText;
    [TextArea] public string progressText;
    [TextArea] public string completeText;

    public ItemData targetItem;
    public int requiredCount = 3;
}
