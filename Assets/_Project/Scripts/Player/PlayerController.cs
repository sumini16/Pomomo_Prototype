using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedPull = -2f;


    [Header("Knockback")]
    [SerializeField] private float knockbackDecay = 5f;


    [Header("Animation")]
    [SerializeField] private Animator animator;



    private CharacterController controller;
    private InputSystem_Actions input;
    private Transform cameraTransform;
    private float velocityY;
    private Vector3 knockbackVelocity;

    public void SetMoveSpeed(float value) => moveSpeed = Mathf.Max(0.1f, value);
    public void SetAnimator(Animator value) => animator = value;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = new InputSystem_Actions();

        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
        else
            Debug.LogError($"{name}: MainCamera 태그가 붙은 카메라를 찾지 못했습니다.");
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {
        // 상점·인벤토리 같은 UI가 열려 있으면 조작을 받지 않습니다.
        if (UIState.IsModalOpen)
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }

        Vector3 moveDirection = ReadMoveDirection();

        RotateTowards(moveDirection);
        ApplyGravity();

        Vector3 velocity = moveDirection * moveSpeed + knockbackVelocity;
        velocity.y = velocityY;

        controller.Move(velocity * Time.deltaTime);

        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);

        // 이동량을 애니메이터에 넘깁니다. 0이면 Idle, 크면 Walk로 전이됩니다.
        if (animator != null)
            animator.SetFloat("Speed", moveDirection.magnitude);
    }

    /// <summary>입력을 카메라 기준 수평 방향 벡터로 변환합니다.</summary>
    private Vector3 ReadMoveDirection()
    {
        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude < 0.01f)
            return Vector3.zero;

        // 카메라는 아래를 내려다보고 있어 forward에 y 성분이 섞여 있습니다.
        // y를 제거해 수평면에 눕힌 뒤, 줄어든 길이를 다시 1로 되돌립니다.
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = camForward * moveInput.y + camRight * moveInput.x;

        // 대각선 입력이 1보다 길어지는 경우에만 정규화.
        // 항상 정규화하면 게임패드의 미세 입력이 최대 속도로 뭉개집니다.
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        return direction;
    }

    /// <summary>이동 방향을 향해 부드럽게 회전합니다.</summary>
    private void RotateTowards(Vector3 direction)
    {
        // 0벡터를 LookRotation에 넘기면 경고가 발생합니다.
        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocityY < 0f)
            velocityY = groundedPull;
        else
            velocityY += gravity * Time.deltaTime;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction * force;
        Debug.Log($"넉백 적용 {direction} × {force}");
    }
}