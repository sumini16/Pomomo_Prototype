using UnityEngine;
public class ChaseState : State
{
    private readonly EnemyAI ai;

    public ChaseState(EnemyAI ai)
    {
        this.ai = ai;
    }

    public override void Tick()
    {
        //Debug.Log($"Chase dist={ai.DistanceToTarget():F2} (attackRange={ai.AttackRange})");
        float distance = ai.DistanceToTarget();

        // 너무 멀어지면 포기하고 순찰로 복귀
        if (distance > ai.LoseRange)
        {
            ai.Machine.ChangeState(ai.Patrol);
            return;
        }

        // 사거리 안이면 멈춤 (2단계에서 Attack으로 교체할 자리)
        if (distance <= ai.AttackRange)
        {
            ai.Machine.ChangeState(ai.Attack);
            return;
        }

        ai.MoveTowards(ai.Target.position, ai.ChaseSpeed);
    }
}