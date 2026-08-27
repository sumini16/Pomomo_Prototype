using System;

/// <summary>
/// 전투 사실을 알리는 창구. QuestEvents·DialogueEvents와 같은 역할입니다.
/// 적은 누가 듣는지 모르고, KillTracker는 개별 적을 추적하지 않습니다.
/// </summary>
public static class CombatEvents
{
    /// <summary>적이 사망했음. (죽은 적의 종류)</summary>
    public static event Action<EnemyData> OnEnemyKilled;

    public static void EnemyKilled(EnemyData enemy) => OnEnemyKilled?.Invoke(enemy);
}
