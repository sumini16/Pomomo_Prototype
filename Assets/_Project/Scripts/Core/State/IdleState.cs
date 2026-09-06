using UnityEngine;
public class IdleState : State
{
    private readonly CompanionAI ai;

    public IdleState(CompanionAI ai)
    {
        this.ai = ai;
    }

    public override void Tick()
    {
        if (ai.DistanceToTarget() > ai.FollowDistance)
            ai.Machine.ChangeState(ai.Follow);

        
    }
}