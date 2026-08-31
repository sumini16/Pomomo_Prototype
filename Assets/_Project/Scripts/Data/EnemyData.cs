using UnityEngine;

/// <summary>
/// 적 '종류'의 설정 데이터.
///
/// 종류마다 같은 값(체력·속도·사거리)은 여기에 두고,
/// 개체마다 다른 값(순찰 지점)은 씬의 EnemyAI에 남깁니다.
///
/// 현재 체력 같은 런타임 상태는 여기 두지 않습니다(2.14).
/// ScriptableObject는 에셋이라 값이 파일에 기록되고, 같은 에셋을 참조하는
/// 개체들이 상태를 공유하게 됩니다.
/// </summary>
[CreateAssetMenu(fileName = "Enemy_", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Tooltip("세이브 파일에 기록되는 고유 키. 한 번 정하면 바꾸지 않습니다.")]
    public string id;

    public string displayName;

    [Header("체력")]
    [Min(1)] public int maxHealth = 100;

    [Header("이동")]
    [Min(0f)] public float patrolSpeed = 2f;
    [Min(0f)] public float chaseSpeed = 4.5f;
    [Min(0f)] public float rotationSpeed = 10f;

    [Header("사거리")]
    [Tooltip("이 거리 안에 들어오면 추적을 시작합니다.")]
    [Min(0f)] public float detectRange = 6f;

    [Tooltip("이 거리를 벗어나면 추적을 포기합니다. detectRange보다 커야 합니다.")]
    [Min(0f)] public float loseRange = 9f;

    [Tooltip("이 거리 안에서 공격합니다.")]
    [Min(0f)] public float attackRange = 1.5f;

    [Tooltip("공격 중 이 거리를 벗어나면 다시 추적합니다. attackRange보다 커야 합니다.")]
    [Min(0f)] public float attackExitRange = 2.2f;

    [Header("전투")]
    [Min(0)] public int attackDamage = 10;
    [Min(0f)] public float attackCooldown = 1.2f;
    [Min(0f)] public float knockbackForce = 6f;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;

        // 진입·이탈 임계값이 뒤집히면 경계에서 상태가 매 프레임 진동합니다(2.18).
        // 값을 넣는 시점에 막아두면 실행 중에 원인을 찾을 일이 없습니다.
        if (loseRange <= detectRange)
            loseRange = detectRange + 1f;

        if (attackExitRange <= attackRange)
            attackExitRange = attackRange + 0.5f;
    }
}