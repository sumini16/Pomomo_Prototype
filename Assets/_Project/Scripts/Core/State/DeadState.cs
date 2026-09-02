using UnityEngine;

public class DeadState : State
{
    private readonly EnemyAI ai;

    public DeadState(EnemyAI ai)
    {
        this.ai = ai;
    }

    public override void Enter()
    {
        ai.PlayDeathAnimation();

        // 더 이상 공격 판정에 걸리지 않도록 콜라이더를 끕니다.
        // 오브젝트는 사망 모션이 끝날 때까지 남아 있어야 하므로 즉시 파괴하지 않습니다.
        if (ai.TryGetComponent(out Collider collider))
            collider.enabled = false;

        Object.Destroy(ai.gameObject, ai.DeathDelay);
    }

    // Tick 없음  죽은 뒤에는 아무 판단도 하지 않습니다.
}