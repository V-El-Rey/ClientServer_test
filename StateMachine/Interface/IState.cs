public interface IState
{
    Task Handle(StateMachine stateMachine);
}