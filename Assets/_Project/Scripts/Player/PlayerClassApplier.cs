using UnityEngine;

/// <summary>
/// 선택된 직업의 수치를 플레이어 컴포넌트들에 주입합니다.
///
/// 각 컴포넌트가 GameManager를 직접 보게 하면 직업을 아는 곳이 세 군데로 늘어납니다.
/// 주입하는 책임을 이 한 곳에 모아, 컴포넌트들은 "값을 받는" 역할만 하게 했습니다.
/// </summary>
[DefaultExecutionOrder(-50)]   // 각 컴포넌트의 Start보다 먼저
public class PlayerClassApplier : MonoBehaviour
{
    [Tooltip("선택 씬을 거치지 않고 이 씬을 바로 실행할 때 사용할 직업입니다.")]
    [SerializeField] private ClassData fallbackClass;

    [SerializeField] private Health health;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerController controller;

    [Tooltip("직업 모델이 들어갈 자리. 기존 자식은 지우고 새 모델을 붙입니다.")]
    [SerializeField] private Transform modelRoot;

    public ClassData Current { get; private set; }

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (combat == null) combat = GetComponent<PlayerCombat>();
        if (controller == null) controller = GetComponent<PlayerController>();

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

        SwapModel(data.modelPrefab);
    }

    private void SwapModel(GameObject prefab)
    {
        if (prefab == null || modelRoot == null) return;

        // DestroyImmediate가 아닌 Destroy는 프레임 끝에 처리되므로,
        // 남아 있는 자식이 새 모델과 겹쳐 보이지 않도록 먼저 꺼둡니다.
        for (int i = modelRoot.childCount - 1; i >= 0; i--)
        {
            GameObject old = modelRoot.GetChild(i).gameObject;
            old.SetActive(false);
            Destroy(old);
        }

        GameObject model = Instantiate(prefab, modelRoot);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        // 모델과 함께 Animator도 교체합니다.
        // 이 줄이 없으면 이동·공격 애니메이션이 파괴된 오브젝트를 향해 호출됩니다.
        Animator animator = model.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[PlayerClassApplier] {prefab.name}에 Animator가 없습니다.", this);
            return;
        }

        if (controller != null) controller.SetAnimator(animator);
        if (combat != null) combat.SetAnimator(animator);
    }
}