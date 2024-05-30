public class StateMachine
{
    private IState? _state;

    private Dictionary<string, IState> states;

    public StateMachine()
    {
        states = new Dictionary<string, IState>();
    }

    public void InitializeState(IState stateToAdd, string name)
    {
        states.Add(name, stateToAdd);
    }

    public void TransitionTo(string state)
    {
        _state = states[state];
        _state.Handle(this);
    }
}