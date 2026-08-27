using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 처치 횟수를 종류별로 보관합니다. PlayerInventory와 같은 역할·같은 모양입니다.
///
/// 인벤토리와 달리 처치는 세상에 남는 상태가 없습니다(적은 죽으면 사라짐).
/// 그래서 집계 자체는 피할 수 없지만, 집계를 여기 한 곳에 가두고
/// 그 결과를 '조회 가능한 상태'로 바꿉니다.
/// 덕분에 KillObjective는 이벤트를 구독하지 않고 묻기만 하면 됩니다.
/// </summary>
public class KillTracker : MonoBehaviour
{
    private readonly Dictionary<EnemyData, int> kills = new();

    public event Action OnKillsChanged;
    public IReadOnlyDictionary<EnemyData, int> Kills => kills;

    private void OnEnable() => CombatEvents.OnEnemyKilled += Record;
    private void OnDisable() => CombatEvents.OnEnemyKilled -= Record;

    private void Record(EnemyData enemy)
    {
        if (enemy == null) return;

        kills[enemy] = GetCount(enemy) + 1;
        OnKillsChanged?.Invoke();
    }

    public int GetCount(EnemyData enemy)
    {
        if (enemy == null) return 0;
        return kills.TryGetValue(enemy, out int count) ? count : 0;
    }

    /// <summary>세이브 불러오기용. 집계를 통째로 복원합니다.</summary>
    public void Restore(IEnumerable<KeyValuePair<EnemyData, int>> saved)
    {
        kills.Clear();
        foreach (var pair in saved)
        {
            if (pair.Key != null && pair.Value > 0)
                kills[pair.Key] = pair.Value;
        }
        OnKillsChanged?.Invoke();
    }
}
