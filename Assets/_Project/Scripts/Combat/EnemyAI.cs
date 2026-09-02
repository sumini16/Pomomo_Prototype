using UnityEngine;

/// <summary>
/// 적의 행동을 담당합니다.
///
/// 수치는 하나도 들고 있지 않습니다. 종류별 값은 EnemyData가 갖고,
/// 이 클래스는 그것을 읽어 상태들에게 넘겨주기만 합니다.
/// 적 종류를 추가할 때 작성할 코드가 없고, 에셋만 만들면 됩니다(2.11과 같은 이유).
///
/// 예외는 순찰 지점입니다. 종류가 아니라 '이 개체가 씬 어디를 도는가'라서
/// 데이터가 아니라 인스펙터에 남겨두었습니다.
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("이 적의 종류. 체력·속도·사거리를 여기서 읽습니다.")]
    [SerializeField] private EnemyData enemyData;

    [Header("Patrol (개체마다 다름)")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointTolerance = 0.3f;

    public EnemyData Data => enemyData;

    public string DisplayName =>
        enemyData != null && !string.IsNullOrWhiteSpace(enemyData.displayName)
            ? enemyData.displayName
            : name;

    // ── 데이터에서 읽는 값들 ──
    // 상태(PatrolState 등)는 EnemyData의 존재를 모르고 이 프로퍼티만 봅니다.
    public float PatrolSpeed => enemyData.patrolSpeed;
    public float ChaseSpeed => enemyData.chaseSpeed;
    public float RotationSpeed => enemyData.rotationSpeed;
    public float DetectRange => enemyData.detectRange;
    public float LoseRange => enemyData.loseRange;
    public float AttackRange => enemyData.attackRange;
    public float AttackExitRange => enemyData.attackExitRange;
    public int AttackDamage => enemyData.attackDamage;
    public float AttackCooldown => enemyData.attackCooldown;
    public float KnockbackForce => enemyData.knockbackForce;

    // ── 개체가 갖는 값 ──
    public Transform[] Waypoints => waypoints;
    public float WaypointTolerance => waypointTolerance;

    public Transform Target { get; private set; }
    public Health Health { get; private set; }

    public StateMachine Machine { get; private set; }
    public PatrolState Patrol { get; private set; }
    public ChaseState Chase { get; private set; }
    public AttackState Attack { get; private set; }
    public DeadState Dead { get; private set; }

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string dieTrigger = "Die";

    [Tooltip("사망 모션이 재생될 시간. 이후 오브젝트가 사라집니다.")]
    [SerializeField] private float deathDelay = 1.5f;

    public float DeathDelay => deathDelay;



    private void Awake()
    {
        Health = GetComponent<Health>();

        if (enemyData == null)
        {
            Debug.LogError($"{name}: EnemyData가 할당되지 않았습니다. 비활성화합니다.");
            enabled = false;
            return;
        }

        // 최대 체력도 종류가 결정합니다.
        // Health.Awake보다 먼저 불려도 나중에 불려도 결과가 같게 만들어 두었습니다.
        Health.SetMaxHealth(enemyData.maxHealth);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            Target = player.transform;
        else
            Debug.LogError($"{name}: Player 태그를 가진 오브젝트를 찾지 못했습니다.");

        Machine = new StateMachine();
        Patrol = new PatrolState(this);
        Chase = new ChaseState(this);
        Attack = new AttackState(this);
        Dead = new DeadState(this);
    }

    private void OnEnable() => Health.OnDied += HandleDied;
    private void OnDisable() => Health.OnDied -= HandleDied;

    private void HandleDied()
    {
        Machine.ChangeState(Dead);

        // 누가 듣는지 모른 채 사실만 알립니다. KillTracker가 구독해 집계합니다.
        CombatEvents.EnemyKilled(enemyData);
    }

    private void Start()
    {
        Machine.ChangeState(Patrol);
    }

    private void Update()
    {
        Machine.Tick();
    }

    public float DistanceToTarget()
    {
        if (Target == null) return float.MaxValue;

        Vector3 delta = Target.position - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    /// <summary>지정 방향으로 이동하며 그 방향을 바라봅니다.</summary>
    public void MoveTowards(Vector3 destination, float speed)
    {
        Vector3 delta = destination - transform.position;
        delta.y = 0f;

        float distance = delta.magnitude;
        if (distance < 0.01f) return;

        Vector3 direction = delta / distance;

        transform.position += direction * speed * Time.deltaTime;

        Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, look, RotationSpeed * Time.deltaTime);
    }

    /// <summary>이동 없이 대상 쪽으로 방향만 맞춥니다.</summary>
    public void FaceTarget()
    {
        if (Target == null) return;

        Vector3 delta = Target.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(delta.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, look, RotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, enemyData.loseRange);
    }

    public void PlayAttackAnimation()
    {
        if (animator != null) animator.SetTrigger(attackTrigger);
    }

    public void PlayDeathAnimation()
    {
        if (animator != null) animator.SetTrigger(dieTrigger);
    }
}