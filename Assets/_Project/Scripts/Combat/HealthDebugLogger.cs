using UnityEngine;

[RequireComponent(typeof(Health))]
public class HealthDebugLogger : MonoBehaviour
{
    private Health health;

    private void Awake() => health = GetComponent<Health>();

    private void OnEnable()
    {
        health.OnHealthChanged += HandleHealthChanged;
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= HandleHealthChanged;
        health.OnDied -= HandleDied;
    }

    private void HandleHealthChanged(int current, int max)
        => Debug.Log($"[{name}] 체력 {current}/{max}");

    private void HandleDied()
        => Debug.Log($"[{name}] 사망");
}