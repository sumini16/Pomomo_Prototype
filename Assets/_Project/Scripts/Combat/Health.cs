using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [Tooltip("받는 피해에서 차감됩니다. 직업(ClassData)이 값을 지정합니다.")]
    [SerializeField] private int defense = 0;

    public int Current { get; private set; }
    public int Max => maxHealth;
    public int Defense => defense;
    public bool IsDead => Current <= 0;

    /// <summary>(현재 체력, 최대 체력)</summary>
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private void Awake()
    {
        // SetMaxHealth가 먼저 불렸다면 이미 채워져 있습니다.
        if (Current <= 0) Current = maxHealth;
    }

    /// <summary>
    /// 최대 체력을 밖에서 지정합니다. 적은 EnemyData가, 플레이어는 ClassData가 값을 갖고 있습니다.
    ///
    /// Awake 호출 순서에 기대지 않도록 만들었습니다.
    /// 이 메서드가 Health.Awake보다 먼저 불려도, 나중에 불려도 결과가 같습니다(문제 6).
    /// </summary>
    public void SetMaxHealth(int value, bool refill = true)
    {
        maxHealth = Mathf.Max(1, value);

        if (refill || Current > maxHealth) Current = maxHealth;

        OnHealthChanged?.Invoke(Current, maxHealth);
    }

    /// <summary>방어력을 밖에서 지정합니다. 체력과 달리 현재값이 없어 다시 채울 것이 없습니다.</summary>
    public void SetDefense(int value)
    {
        defense = Mathf.Max(0, value);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;      // 이미 죽었으면 OnDied가 중복 발사되지 않도록
        if (amount <= 0) return; // 음수가 들어와 회복이 되는 것을 막음

        // 방어력이 공격력보다 높아도 최소 1은 들어갑니다.
        // 0이 되면 어떤 공격으로도 죽지 않는 교착 상태가 생깁니다.
        int actual = Mathf.Max(1, amount - defense);

        Current = Mathf.Max(0, Current - actual);

        OnHealthChanged?.Invoke(Current, maxHealth);

        if (IsDead)
            OnDied?.Invoke();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        Current = Mathf.Min(maxHealth, Current + amount);
        OnHealthChanged?.Invoke(Current, maxHealth);
    }
}