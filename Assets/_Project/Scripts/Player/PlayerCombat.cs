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

    private void Awake() => input = new InputSystem_Actions();

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

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

        Debug.Log($"[공격] 감지된 콜라이더 {count}개");          // ← 추가

        int hitCount = 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 toTarget = hits[i].transform.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f) continue;

            float dot = Vector3.Dot(transform.forward, toTarget.normalized);
            bool hasHealth = hits[i].TryGetComponent(out Health targetHealth);

            Debug.Log($"  → {hits[i].name} / 정면성 {dot:F2} / Health {(hasHealth ? "있음" : "없음")}");  // ← 추가

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