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
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, attackRange, hits, targetLayer);

        int hitCount = 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 toTarget = hits[i].transform.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f) continue;

            // 등 뒤의 적은 맞지 않도록 상호작용 판정과 같은 방식
            if (Vector3.Dot(transform.forward, toTarget.normalized) < minFacingDot)
                continue;

            if (hits[i].TryGetComponent(out Health targetHealth))
            {
                targetHealth.TakeDamage(attackDamage);
                hitCount++;
            }
        }

        Debug.Log($"공격  {hitCount}체 명중");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}