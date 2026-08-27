using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("퀘스트 처치 집계에 쓰이는 종류 식별자. 처치형 목표를 쓰려면 필요합니다.")]
    [SerializeField] private EnemyData enemyData;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointTolerance = 0.3f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Ranges")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float detectRange = 6f;
    [Tooltip("이 거리를 벗어나면 추적을 포기합니다. detectRange보다 커야 합니다.")]
    [SerializeField] private float loseRange = 9f;


    [Header("Combat")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float knockbackForce = 6f;

    [Tooltip("공격 상태에서 이 거리를 벗어나면 다시 추적합니다. attackRange보다 커야 합니다.")]
    [SerializeField] private float attackExitRange = 2.2f;

    public float AttackExitRange => attackExitRange;

    public Transform Target { get; private set; }
    public Health Health { get; private set; }

    public Transform[] Waypoints => waypoints;
    public float WaypointTolerance => waypointTolerance;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float RotationSpeed => rotationSpeed;
    public float AttackRange => attackRange;
    public float DetectRange => detectRange;
    public float LoseRange => loseRange;

    public StateMachine Machine { get; private set; }
    public PatrolState Patrol { get; private set; }
    public ChaseState Chase { get; private set; }

    public int AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;
    public float KnockbackForce => knockbackForce;

    public AttackState Attack { get; private set; }
    public DeadState Dead { get; private set; }

    private void OnEnable() => Health.OnDied += HandleDied;
    private void OnDisable() => Health.OnDied -= HandleDied;

    private void HandleDied()
    {
        Machine.ChangeState(Dead);

        // 누가 듣는지 모른 채 사실만 알립니다. KillTracker가 구독해 집계합니다.
        // Health가 중복 사망을 막아주므로 여기서 두 번 발행되지 않습니다.
        CombatEvents.EnemyKilled(enemyData);
    }
    private void Awake()
    {
        Health = GetComponent<Health>();

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
            transform.rotation, look, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }

    /// <summary>이동 없이 대상 쪽으로 방향만 맞춥니다.</summary>
    public void FaceTarget()
    {
        if (Target == null) return;

        Vector3 delta = Target.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(delta.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
    }
}