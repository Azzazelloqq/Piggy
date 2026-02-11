using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Piggy.Code.StateMachine
{
public sealed class StateMachine
{
    private static readonly AsyncLocal<StateMachine> _transitionOwner = new();
    private readonly Dictionary<Type, GameState> _states = new();
    private readonly SemaphoreSlim _transitionGate = new(1, 1);
    private readonly bool _isSubStateMachine;
    private GameState _currentState;
    private bool _isTransitioning;

    public StateMachine(bool isSubStateMachine = false)
    {
        _isSubStateMachine = isSubStateMachine;
    }

    public GameState CurrentState => _currentState;

    public GameState CurrentSubState => _currentState == null
        ? null
        : GetSubStateMachine(_currentState).CurrentState;

    public bool IsTransitioning => _isTransitioning;

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

    public async UniTask ShutdownSubStatesAsync(CancellationToken cancellationToken = default)
    {
        if (TryGetSubStateMachine(out var subStateMachine))
        {
            await subStateMachine.ShutdownAsync(cancellationToken);
        }
    }

    public UniTask ChangeStateAsync<TState, TSceneContext>(
        TSceneContext sceneContext = default,
        bool force = false,
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

            return subStateMachine.ChangeStateAsyncInternal(subState, sceneContext, force, cancellationToken);
        }

        if (!_states.TryGetValue(typeof(TState), out var nextState))
        {
            throw new InvalidOperationException($"State not registered: {typeof(TState).Name}");
        }

        return ChangeStateAsyncInternal(nextState, sceneContext, force, cancellationToken);
    }

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
        CancellationToken cancellationToken) where T : struct, IGameStateContext
    {
        return RunTransitionAsync(async ct =>
        {
            ct.ThrowIfCancellationRequested();
            if (!force && ReferenceEquals(_currentState, nextState))
            {
                return;
            }

            if (_currentState != null)
            {
                await ShutdownSubStatesAsync(ct);
                await ((IGameState)_currentState).ExitAsync(ct);
            }

            _currentState = null;
            ct.ThrowIfCancellationRequested();

            await ((IGameState)nextState).EnterAsync(gameStateContext, ct);
            
            _currentState = nextState;
        }, cancellationToken);
    }

    private async UniTask RunTransitionAsync(
        Func<CancellationToken, UniTask> transition,
        CancellationToken cancellationToken)
    {
        if (_transitionOwner.Value == this)
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

    private static StateMachine GetSubStateMachine(GameState state)
    {
        return ((IGameState)state).SubStateMachine;
    }
}
}