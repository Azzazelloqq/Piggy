using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Piggy.Code.StateMachine
{
/// <summary>
/// Default implementation of <see cref="IStateMachine"/> with support for sub-states.
/// </summary>
public sealed class StateMachine : IStateMachine
{    
    /// <summary>
    /// Gets the currently active top-level state, or null when idle.
    /// </summary>
    public GameState CurrentState => _currentState;

    /// <summary>
    /// Gets the currently active sub-state, or null when none.
    /// </summary>
    public GameState CurrentSubState => _currentState == null
        ? null
        : GetSubStateMachine(_currentState).CurrentState;

    /// <summary>
    /// Gets a value indicating whether a transition is in progress.
    /// </summary>
    public bool IsTransitioning => _isTransitioning;

    private static readonly AsyncLocal<StateMachine> _transitionOwner = new();
    private readonly Dictionary<Type, GameState> _states = new();
    private readonly SemaphoreSlim _transitionGate = new(1, 1);
    private readonly bool _isSubStateMachine;
    private GameState _currentState;
    private bool _isTransitioning;
    
    /// <summary>
    /// Creates a new state machine.
    /// </summary>
    /// <param name="isSubStateMachine">Whether this instance is used as a sub-state machine.</param>
    public StateMachine(bool isSubStateMachine = false)
    {
        _isSubStateMachine = isSubStateMachine;
    }
    
    /// <inheritdoc />
    public void Register(GameState state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var type = state.GetType();
        if (!_states.TryAdd(type, state))
        {
            throw new InvalidOperationException($"State already registered: {type.Name}");
        }
    }

    /// <inheritdoc />
    public bool TryGetState<TState>(out TState state) where TState : GameState
    {
        if (_states.TryGetValue(typeof(TState), out var stored))
        {
            state = stored as TState;
            return state != null;
        }

        state = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetSubStateMachine(out StateMachine subStateMachine)
    {
        if (_currentState != null)
        {
            subStateMachine = GetSubStateMachine(_currentState);
            return true;
        }

        subStateMachine = null;
        return false;
    }

    /// <inheritdoc />
    public async UniTask ShutdownSubStatesAsync(CancellationToken cancellationToken = default)
    {
        if (TryGetSubStateMachine(out var subStateMachine))
        {
            await subStateMachine.ShutdownAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public UniTask ChangeStateAsync<TState, TSceneContext>(
        TSceneContext sceneContext = default,
        bool force = false,
        StateTransitionMode transitionMode = StateTransitionMode.Sequential,
        CancellationToken cancellationToken = default)
        where TState : GameState 
        where TSceneContext : struct, IGameStateContext
    {
        if (!_isSubStateMachine && typeof(ISubState).IsAssignableFrom(typeof(TState)))
        {
            if (_currentState == null)
            {
                throw new InvalidOperationException("Cannot change sub-state without current state.");
            }

            var subStateMachine = GetSubStateMachine(_currentState);
            if (!subStateMachine.TryGetState<TState>(out var subState))
            {
                throw new InvalidOperationException($"Sub-state not registered: {typeof(TState).Name}");
            }

            return subStateMachine.ChangeStateAsyncInternal(
                subState,
                sceneContext,
                force,
                transitionMode,
                cancellationToken);
        }

        if (!_states.TryGetValue(typeof(TState), out var nextState))
        {
            throw new InvalidOperationException($"State not registered: {typeof(TState).Name}");
        }

        return ChangeStateAsyncInternal(nextState, sceneContext, force, transitionMode, cancellationToken);
    }

    /// <inheritdoc />
    public UniTask ShutdownAsync(CancellationToken cancellationToken = default)
    {
        return RunTransitionAsync(async ct =>
        {
            ct.ThrowIfCancellationRequested();
            if (_currentState == null)
            {
                return;
            }

            await ShutdownSubStatesAsync(ct);
            await ((IGameState)_currentState).ExitAsync(ct);
            _currentState = null;
        }, cancellationToken);
    }

    private UniTask ChangeStateAsyncInternal<T>(
        GameState nextState,
        T gameStateContext,
        bool force,
        StateTransitionMode transitionMode,
        CancellationToken cancellationToken) where T : struct, IGameStateContext
    {
        return RunTransitionAsync(async ct =>
        {
            ct.ThrowIfCancellationRequested();
            if (!force && ReferenceEquals(_currentState, nextState))
            {
                return;
            }

            var previousState = _currentState;
            if (previousState != null)
            {
                await ShutdownSubStatesAsync(ct);
            }

            var useOverlap = transitionMode == StateTransitionMode.OverlapExitEnter
                && previousState != null
                && !ReferenceEquals(previousState, nextState);

            if (useOverlap)
            {
                var exitTask = ((IGameState)previousState).ExitAsync(ct);

                _currentState = null;
                ct.ThrowIfCancellationRequested();

                var enterTask = ((IGameState)nextState).EnterAsync(gameStateContext, ct);

                await UniTask.WhenAll(exitTask, enterTask);
                _currentState = nextState;
                return;
            }

            if (previousState != null)
            {
                await ((IGameState)previousState).ExitAsync(ct);
            }

            _currentState = null;
            ct.ThrowIfCancellationRequested();

            await ((IGameState)nextState).EnterAsync(gameStateContext, ct);

            _currentState = nextState;
        }, cancellationToken);
    }

    /// <summary>
    /// Runs a transition under a gate to prevent concurrent or re-entrant transitions.
    /// </summary>
    private async UniTask RunTransitionAsync(
        Func<CancellationToken, UniTask> transition,
        CancellationToken cancellationToken)
    {
        if (_isTransitioning && _transitionOwner.Value == this)
        {
            throw new InvalidOperationException("Re-entrant transitions are not allowed.");
        }

        await _transitionGate.WaitAsync(cancellationToken);
        var previousOwner = _transitionOwner.Value;
        _transitionOwner.Value = this;
        _isTransitioning = true;
        try
        {
            await transition(cancellationToken);
        }
        finally
        {
            _isTransitioning = false;
            _transitionOwner.Value = previousOwner;
            _transitionGate.Release();
        }
    }

    /// <summary>
    /// Gets the sub-state machine for the specified state.
    /// </summary>
    private static StateMachine GetSubStateMachine(GameState state)
    {
        return ((IGameState)state).SubStateMachine;
    }
}
}