namespace Piggy.Code.StateMachine
{
    /// <summary>
    /// Base class for sub-states that are managed by a parent state's sub-state machine.
    /// </summary>
    public abstract class GameSubState : GameState, ISubState
    {
    }
}
