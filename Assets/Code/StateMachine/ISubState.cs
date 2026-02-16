namespace Piggy.Code.StateMachine
{
    /// <summary>
    /// Marker interface for states that are managed by a parent state's sub-state machine.
    /// </summary>
    public interface ISubState : IState
    {
    }
}
