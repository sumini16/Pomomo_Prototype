using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;
}
