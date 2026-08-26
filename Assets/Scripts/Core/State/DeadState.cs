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
        Debug.Log($"{ai.name} 사망");
        // 나중에 사망 애니메이션·이펙트를 여기서 재생
    }

    // Tick 없음  죽은 뒤에는 아무 판단도 하지 않습니다.
}