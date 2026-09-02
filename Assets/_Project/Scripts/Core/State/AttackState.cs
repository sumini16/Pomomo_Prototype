using UnityEngine;

public class AttackState : State
{
    private readonly EnemyAI ai;
    private float lastAttackTime;

    public AttackState(EnemyAI ai)
    {
        this.ai = ai;
    }

    public override void Enter()
    {
        // 재진입 시 쿨다운이 초기화되지 않도록, 남은 쿨다운이 없을 때만 한 박자 쉼
        if (Time.time - lastAttackTime > ai.AttackCooldown)
            lastAttackTime = Time.time - ai.AttackCooldown * 0.5f;
    }

    public override void Tick()
    {
        // 사거리를 벗어나면 다시 추적
        if (ai.DistanceToTarget() > ai.AttackExitRange)   // AttackRange → AttackExitRange
        {
            ai.Machine.ChangeState(ai.Chase);
            return;
        }

        ai.FaceTarget();   // 공격 중에도 플레이어를 바라보게

        if (Time.time - lastAttackTime < ai.AttackCooldown)
            return;

        lastAttackTime = Time.time;
        PerformAttack();
    }

    private void PerformAttack()
    {
        if (ai.Target == null) return;

        ai.PlayAttackAnimation();

        if (ai.Target.TryGetComponent(out Health targetHealth))
            targetHealth.TakeDamage(ai.AttackDamage);

        if (ai.Target.TryGetComponent(out PlayerController player))
        {
            Vector3 direction = ai.Target.position - ai.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
                player.ApplyKnockback(direction.normalized, ai.KnockbackForce);
        }

        
    }
}