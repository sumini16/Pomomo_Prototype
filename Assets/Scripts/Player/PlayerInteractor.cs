using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactableLayer;

    [Tooltip("이 값보다 정면성이 낮으면 후보에서 제외합니다. 0=좌우 90도, 0.3=약 70도, 0.7=약 45도")]
    [SerializeField, Range(-1f, 1f)] private float minFacingDot = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private InputSystem_Actions input;

    /// <summary>현재 선택된 상호작용 대상. 없으면 null.</summary>
    public Interactable CurrentTarget { get; private set; }

    // OverlapSphere 결과를 담을 버퍼. 매 프레임 배열을 새로 만들지 않기 위해 미리 확보합니다.
    private readonly Collider[] candidates = new Collider[16];

    private void Awake()
    {
        input = new InputSystem_Actions();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {
        CurrentTarget = FindBestTarget();

        if (CurrentTarget != null && input.Player.Interact.WasPressedThisFrame())
            CurrentTarget.Interact(gameObject);
    }

    /// <summary>
    /// 주변 후보 중 "앞쪽에 있으면서 가장 가까운" 대상을 고릅니다.
    /// 정면성(내적)은 걸러내는 기준으로, 거리는 순위를 정하는 기준으로 사용합니다.
    /// </summary>
    private Interactable FindBestTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, interactRange, candidates, interactableLayer);

        Interactable best = null;
        float bestDistanceSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (!candidates[i].TryGetComponent(out Interactable interactable))
                continue;

            Vector3 toTarget = interactable.transform.position - transform.position;

            // 높이 차이는 정면 판정에서 제외합니다.
            // 발밑이나 머리 위의 물건이 "뒤에 있다"고 잘못 판정되는 것을 막기 위해서입니다.
            Vector3 flatToTarget = toTarget;
            flatToTarget.y = 0f;

            if (flatToTarget.sqrMagnitude < 0.0001f)
                continue;

            float facing = Vector3.Dot(transform.forward, flatToTarget.normalized);
            if (facing < minFacingDot)
                continue;

            float distanceSqr = toTarget.sqrMagnitude;
            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                best = interactable;
            }
        }

        return best;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        if (CurrentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, CurrentTarget.transform.position);
        }
    }
}