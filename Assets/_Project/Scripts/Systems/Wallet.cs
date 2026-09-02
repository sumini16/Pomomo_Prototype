using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField, Min(0)] private int startingGold;

    [Header("Runtime Debug")]
    [SerializeField] private int currentGold;

    public int Gold => currentGold;

    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        currentGold = startingGold;
    }

    public void Add(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[Wallet] 지급 실패: amount={amount}");
            return;
        }

        int before = currentGold;
        currentGold += amount;

        Debug.Log($"[Wallet] 골드 지급: {before} → {currentGold} (+{amount})");
        OnGoldChanged?.Invoke(currentGold);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || currentGold < amount)
            return false;

        currentGold -= amount;
        OnGoldChanged?.Invoke(currentGold);
        return true;
    }

    /// <summary>
    /// 저장된 값으로 되돌립니다. Add/TrySpend와 달리 검증 없이 그대로 덮어씁니다.
    /// 복원은 거래가 아니라 과거 상태의 재현이므로 같은 규칙을 적용하지 않습니다.
    /// </summary>
    public void Restore(int gold)
    {
        currentGold = Mathf.Max(0, gold);
        OnGoldChanged?.Invoke(currentGold);
    }
}