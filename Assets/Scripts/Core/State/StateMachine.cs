
public class StateMachine
{
    public State Current { get; private set; }

    public void ChangeState(State next)
    {
        if (Current == next) return;
        // Exit → 교체 → Enter 순서
        Current?.Exit();
        Current = next;
        Current?.Enter();
    }

    public void Tick()
    {
        Current?.Tick();
    }
}