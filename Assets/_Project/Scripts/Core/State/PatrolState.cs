using UnityEngine;

public class PatrolState : State
{
    private readonly EnemyAI ai;
    private int currentIndex;

    public PatrolState(EnemyAI ai)
    {
        this.ai = ai;
    }

    public override void Tick()
    {
        // 플레이어를 발견하면 추적으로
        if (ai.DistanceToTarget() <= ai.DetectRange)
        {
            ai.Machine.ChangeState(ai.Chase);
            return;
        }

        if (ai.Waypoints == null || ai.Waypoints.Length == 0)
            return;

        Transform destination = ai.Waypoints[currentIndex];
        if (destination == null) return;

        ai.MoveTowards(destination.position, ai.PatrolSpeed);

        // 도착 판정  높이 차이는 무시
        Vector3 delta = destination.position - ai.transform.position;
        delta.y = 0f;

        if (delta.magnitude <= ai.WaypointTolerance)
            currentIndex = (currentIndex + 1) % ai.Waypoints.Length;
    }
}