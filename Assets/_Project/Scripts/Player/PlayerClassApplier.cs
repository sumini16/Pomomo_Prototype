using UnityEngine;

/// <summary>
/// 선택된 직업의 수치와 기본 장비를 플레이어에 적용합니다.
///
/// 각 컴포넌트가 GameManager를 직접 보게 하면 직업을 아는 곳이 네 군데로 늘어납니다.
/// 주입하는 책임을 이 한 곳에 모아, 나머지는 값을 받는 역할만 하게 했습니다.
///
/// 장비는 ClassData가 프리팹으로 들고 있습니다.
/// 씬에 미리 배치해 켜고 끄는 방법도 검토했지만, 그러면 직업을 추가할 때마다
/// 에셋과 씬을 함께 고쳐야 합니다. 직업 추가가 에셋 하나로 끝나도록 생성 방식을 택했습니다.
/// </summary>
[DefaultExecutionOrder(-50)]
public class PlayerClassApplier : MonoBehaviour
{
    [Tooltip("선택 씬을 거치지 않고 이 씬을 바로 실행할 때 사용할 직업입니다.")]
    [SerializeField] private ClassData fallbackClass;

    [Header("적용 대상")]
    [SerializeField] private Health health;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerController controller;
    [SerializeField] private PlayerInventory inventory;

    [Header("장착 지점")]
    [Tooltip("모델의 handslot.r 본. Hierarchy 검색창에 handslot을 치면 찾을 수 있습니다.")]
    [SerializeField] private Transform rightHandSlot;
    [SerializeField] private Transform leftHandSlot;

    public ClassData Current { get; private set; }

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (combat == null) combat = GetComponent<PlayerCombat>();
        if (controller == null) controller = GetComponent<PlayerController>();
        if (inventory == null) inventory = GetComponent<PlayerInventory>();

        ClassData data = GameManager.Instance != null ? GameManager.Instance.SelectedClass : null;
        if (data == null) data = fallbackClass;

        if (data == null)
        {
            Debug.LogWarning("[PlayerClassApplier] 적용할 직업이 없습니다. 기본값으로 진행합니다.", this);
            return;
        }

        Apply(data);
    }

    private void Apply(ClassData data)
    {
        Current = data;

        if (health != null)
        {
            health.SetMaxHealth(data.maxHealth);
            health.SetDefense(data.defense);
        }

        if (combat != null) combat.SetAttackDamage(data.attackDamage);
        if (controller != null) controller.SetMoveSpeed(data.moveSpeed);

        Equip(data.weaponPrefab, rightHandSlot);
        Equip(data.offhandPrefab, leftHandSlot);

        if (data.starterItem != null && inventory != null)
            inventory.Add(data.starterItem, 1);
    }

    /// <summary>장착 슬롯을 비우고 장비를 생성합니다. prefab이 null이면 비우기만 합니다.</summary>
    private void Equip(GameObject prefab, Transform slot)
    {
        if (slot == null) return;

        for (int i = slot.childCount - 1; i >= 0; i--)
            Destroy(slot.GetChild(i).gameObject);

        if (prefab == null) return;

        // Instantiate(prefab, slot)은 프리팹에 저장된 로컬 좌표를 그대로 씁니다.
        // 손에 맞춰 조정한 위치·회전이 프리팹에 들어 있으므로 여기서 리셋하지 않습니다.
        Instantiate(prefab, slot);
    }
}