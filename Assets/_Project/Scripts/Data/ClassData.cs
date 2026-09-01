using UnityEngine;

/// <summary>
/// 직업 하나의 설정값. 씬과 무관하게 존재하는 데이터이므로 ScriptableObject로 둡니다.
/// id는 세이브 파일이 에셋을 다시 찾기 위한 열쇠입니다  파일명을 바꿔도 저장이 깨지지 않도록 분리했습니다.
/// </summary>
[CreateAssetMenu(fileName = "Class_", menuName = "Game/Class Data")]
public class ClassData : ScriptableObject
{
    [Header("식별")]
    [Tooltip("세이브 파일에 기록되는 값. 한 번 정하면 바꾸지 않습니다.")]
    public string id;

    [Header("표시")]
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("외형")]
    public GameObject modelPrefab;

    [Header("능력치")]
    public int maxHealth;
    public int attackDamage;
    public float moveSpeed;
    [Tooltip("받는 피해에서 차감됩니다.")]
    public int defense;

    [Header("장비")]
    [Tooltip("오른손(handslot.r)에 생성할 무기 프리팹입니다.")]
    public GameObject weaponPrefab;

    [Tooltip("왼손(handslot.l)에 생성할 프리팹입니다. 없으면 비워둡니다.")]
    public GameObject offhandPrefab;

    [Tooltip("시작 시 인벤토리에 지급할 아이템입니다.")]
    public ItemData starterItem;

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        attackDamage = Mathf.Max(0, attackDamage);
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        defense = Mathf.Max(0, defense);
    }
}