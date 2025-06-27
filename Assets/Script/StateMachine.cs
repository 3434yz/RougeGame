public class StateMachine
{
    public EntityState _currentState { get; private set; }

    public void Initialize(EntityState startState)
    {
        _currentState = startState;
        _currentState.Enter();
    }

    public void ChangeState(EntityState state)
    {
        _currentState.Exit();
        _currentState = state;
        _currentState.Enter();
    }

    public int Update()
    {
        return _currentState.Update();
    }
}