using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask targetLayer;

    [Tooltip("이 값보다 정면성이 낮은 대상은 맞지 않습니다.")]
    [SerializeField, Range(-1f, 1f)] private float minFacingDot = 0.2f;
    [SerializeField] private Animator animator;
    private InputSystem_Actions input;
    private float lastAttackTime;

    private readonly Collider[] hits = new Collider[16];

    [Tooltip("피격 경직 중 공격을 막습니다. 비워두면 같은 오브젝트에서 찾습니다.")]
    [SerializeField] private HitReaction hitReaction;


    private void Awake()
    {
        input = new InputSystem_Actions();
        if (hitReaction == null) hitReaction = GetComponent<HitReaction>();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    public void SetAttackDamage(int value) => attackDamage = Mathf.Max(0, value);
    public void SetAnimator(Animator value) => animator = value;


    private void Update()
    {
        // 상점·인벤토리가 열려 있으면 조작을 받지 않습니다.
        if (UIState.IsModalOpen) return;

        // UI 위를 클릭한 경우는 공격이 아닙니다.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 맞는 도중에는 공격할 수 없습니다.
        if (hitReaction != null && hitReaction.IsStunned) return;

        if (!input.Player.Attack.WasPressedThisFrame()) return;
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        PerformAttack();
    }
    private void PerformAttack()
    {
        if (animator != null) animator.SetTrigger("Attack");

        int count = Physics.OverlapSphereNonAlloc(
            transform.position, attackRange, hits, targetLayer);

      

        int hitCount = 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 toTarget = hits[i].transform.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f) continue;

            float dot = Vector3.Dot(transform.forward, toTarget.normalized);
            bool hasHealth = hits[i].TryGetComponent(out Health targetHealth);

         

            if (dot < minFacingDot) continue;
            if (!hasHealth) continue;

            targetHealth.TakeDamage(attackDamage);
            hitCount++;
        }

        Debug.Log($"공격  {hitCount}체 명중");
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}