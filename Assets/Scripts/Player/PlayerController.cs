using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedPull = -2f;

    private CharacterController controller;
    private InputSystem_Actions input;
    private float velocityY;

    private void Awake()
    {
        controller = GetComponent<CharacterController>(); // 캐릭터 컨트롤러 컴포넌트 가져오기
        input = new InputSystem_Actions();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {
        Vector3 horizontal = ReadMoveDirection();
        ApplyGravity();

        Vector3 velocity = horizontal * moveSpeed;
        velocity.y = velocityY;

        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>입력을 월드 기준 수평 방향 벡터로 변환합니다.</summary>
    private Vector3 ReadMoveDirection()
    {
        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);

        // 대각선 입력이 1보다 길어지는 경우에만 정규화.
        // 항상 정규화하면 게임패드의 미세 입력(살살 밀기)이 최대 속도로 뭉개집니다.
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        return direction;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocityY < 0f)
            velocityY = groundedPull;
        else
            velocityY += gravity * Time.deltaTime;
    }
}
