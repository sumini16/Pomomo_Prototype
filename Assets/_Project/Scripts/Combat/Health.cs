using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public int Current { get; private set; }
    public int Max => maxHealth;
    public bool IsDead => Current <= 0;

    /// <summary>(현재 체력, 최대 체력)</summary>
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;      // 이미 죽었으면 OnDied가 중복 발사되지 않도록
        if (amount <= 0) return; // 음수가 들어와 회복이 되는 것을 막음

        Current = Mathf.Max(0, Current - amount);

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