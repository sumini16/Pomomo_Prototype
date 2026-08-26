using UnityEngine;

public class FollowState : State
{
    private readonly CompanionAI ai;

    public FollowState(CompanionAI ai)
    {
        this.ai = ai;
    }

    public override void Tick()
    {
        Vector3 toPlayer = ai.Target.position - ai.transform.position;
        toPlayer.y = 0f;

        float distance = toPlayer.magnitude;

        // 원하는 간격보다 멀 때만 전진하고, 그때만 방향도 맞춤
        if (distance > ai.DesiredDistance)
        {
            Vector3 direction = toPlayer / distance;

            ai.transform.position += direction * ai.MoveSpeed * Time.deltaTime;

            Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
            ai.transform.rotation = Quaternion.Slerp(
                ai.transform.rotation, look, ai.RotationSpeed * Time.deltaTime);
        }

        if (distance <= ai.StopDistance)
            ai.Machine.ChangeState(ai.Idle);
    }
}