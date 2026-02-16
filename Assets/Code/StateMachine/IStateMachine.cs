using System.Threading;
using Cysharp.Threading.Tasks;

namespace Piggy.Code.StateMachine
{
/// <summary>
/// Defines a state machine that manages <see cref="GameState"/> transitions
/// and optional sub-state routing.
/// </summary>
public interface IStateMachine
{
    /// <summary>
    /// Gets the currently active top-level state, or null when idle.
    /// </summary>
    GameState CurrentState { get; }

    /// <summary>
    /// Gets the currently active sub-state of <see cref="CurrentState"/>, or null when none.
    /// </summary>
    GameState CurrentSubState { get; }

    /// <summary>
    /// Gets a value indicating whether a transition is in progress.
    /// </summary>
    bool IsTransitioning { get; }

    /// <summary>
    /// Registers a state instance by its runtime type.
    /// </summary>
    void Register(GameState state);

    /// <summary>
    /// Tries to retrieve a registered state instance by type.
    /// </summary>
    bool TryGetState<TState>(out TState state) where TState : GameState;

    /// <summary>
    /// Tries to get the sub-state machine of the current state.
    /// </summary>
    bool TryGetSubStateMachine(out StateMachine subStateMachine);

    /// <summary>
    /// Shuts down the active sub-state machine, if any.
    /// </summary>
    UniTask ShutdownSubStatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes to a state of type <typeparamref name="TState"/>.
    /// If <typeparamref name="TState"/> implements <see cref="ISubState"/> and this instance is
    /// a parent machine, the transition is routed to the current state's sub-state machine.
    /// </summary>
    UniTask ChangeStateAsync<TState, TSceneContext>(
        TSceneContext sceneContext = default,
        bool force = false,
        CancellationToken cancellationToken = default)
        where TState : GameState
        where TSceneContext : struct, IGameStateContext;

    /// <summary>
    /// Exits the current state and clears it, also shutting down any active sub-states.
    /// </summary>
    UniTask ShutdownAsync(CancellationToken cancellationToken = default);
}
}