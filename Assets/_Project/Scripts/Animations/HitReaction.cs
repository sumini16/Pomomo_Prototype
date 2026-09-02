using UnityEngine;

/// <summary>
/// 체력이 줄어들 때 피격 모션을 재생하고, 짧은 경직 시간을 만듭니다.
/// Health만 참조하므로 플레이어와 적 모두에 붙습니다.
/// </summary>
[RequireComponent(typeof(Health))]
public class HitReaction : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";

    [Tooltip("피격 후 이 시간 동안 공격할 수 없습니다.")]
    [SerializeField] private float stunDuration = 0.4f;

    private Health health;
    private int lastHealth = -1;
    private float stunUntil;

    /// <summary>피격 경직 중인가. 공격 입력을 막는 쪽에서 참조합니다.</summary>
    public bool IsStunned => Time.time < stunUntil;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        // 구독 전에 이미 지나간 변경(SetMaxHealth 등)을 놓치지 않도록 현재값에서 시작합니다.
        // 이벤트가 채워주기를 기다리면 첫 피격이 통째로 무시됩니다.
        lastHealth = health.Current;

        health.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable() => health.OnHealthChanged -= HandleHealthChanged;

    private void HandleHealthChanged(int current, int max)
    {
        // 회복이나 최대 체력 설정 때도 이 이벤트가 오므로, 줄어든 경우만 반응합니다.
        bool damaged = lastHealth >= 0 && current < lastHealth;
        lastHealth = current;

        if (!damaged || current <= 0) return;   // 죽은 프레임은 사망 처리가 맡습니다

        stunUntil = Time.time + stunDuration;

        if (animator != null) animator.SetTrigger(hitTrigger);
    }
}