
public class IdleTestState : IState
{
    public Task Handle(StateMachine stateMachine)
    {
        return Task.Delay(-1);
    }
}