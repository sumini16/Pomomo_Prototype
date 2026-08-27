using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Tooltip("세이브 파일에 기록되는 고유 키. 한 번 정하면 바꾸지 않습니다.")]
    public string id;

    public string displayName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("상점")]
    [Min(0)] public int buyPrice;
    [Min(0)] public int sellPrice;

    private void OnValidate()
    {
        // 비워두면 에셋 파일명을 기본값으로 씁니다.
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
}
