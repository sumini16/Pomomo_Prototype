using UnityEngine;

public class CompanionAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [Tooltip("플레이어와 유지할 간격. 이보다 가까우면 전진하지 않습니다.")]
    [SerializeField] private float desiredDistance = 1.2f;

    [Header("Distances")]
    [Tooltip("이보다 멀어지면 따라가기 시작합니다.")]
    [SerializeField] private float followDistance = 2.5f;
    [Tooltip("이보다 가까워지면 멈춥니다. followDistance보다 작아야 합니다.")]
    [SerializeField] private float stopDistance = 1.5f;

    public Transform Target => target;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float DesiredDistance => desiredDistance;
    public float FollowDistance => followDistance;
    public float StopDistance => stopDistance;

    public StateMachine Machine { get; private set; }
    public IdleState Idle { get; private set; }
    public FollowState Follow { get; private set; }

    private void Awake()
    {
        Machine = new StateMachine();
        Idle = new IdleState(this);
        Follow = new FollowState(this);

        if (target == null)
            Debug.LogError($"{name}: 따라갈 대상(Target)이 지정되지 않았습니다.");
    }

    private void Start()
    {
        Machine.ChangeState(Idle);
    }

    private void Update()
    {
        Machine.Tick();
    }

    /// <summary>대상과의 수평 거리. 높이 차이는 무시합니다.</summary>
    public float DistanceToTarget()
    {
        Vector3 delta = target.position - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }
}