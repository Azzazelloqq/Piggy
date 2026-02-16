namespace Piggy.Code.StateMachine
{
    /// <summary>
    /// Controls how the state machine performs state transitions.
    /// </summary>
    public enum StateTransitionMode
    {
        /// <summary>
        /// Exits the current state, then enters the next state.
        /// </summary>
        Sequential = 0,

        /// <summary>
        /// Exits the current state and enters the next state concurrently.
        /// </summary>
        OverlapExitEnter = 1
    }
}
