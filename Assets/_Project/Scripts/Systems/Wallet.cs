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
}