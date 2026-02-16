# StateMachine usage examples

## Example 1: basic states with scene context

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Piggy.Code.StateMachine;

public readonly struct SceneContext : IGameStateContext
{
    public readonly string SceneName;

    public SceneContext(string sceneName)
    {
        SceneName = sceneName;
    }
}

public sealed class GameStateDriver : MonoBehaviour
{
    private readonly StateMachine _stateMachine = new StateMachine();
    private CancellationTokenSource _destroyCts;

    private void Awake()
    {
        _destroyCts = new CancellationTokenSource();

        _stateMachine.Register(new BootState());
        _stateMachine.Register(new MenuState());
    }

    private async void Start()
    {
        var context = new SceneContext("Boot");
        await _stateMachine.ChangeStateAsync<BootState, SceneContext>(context, cancellationToken: _destroyCts.Token);
        await _stateMachine.ChangeStateAsync<MenuState, SceneContext>(context, cancellationToken: _destroyCts.Token);
    }

    private void OnDestroy()
    {
        _destroyCts.Cancel();
        _destroyCts.Dispose();
    }
}

public sealed class BootState : GameState
{
    protected override async UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken ct)
        where T : struct, IGameStateContext
    {
        // Warmup or load something here.
        await UniTask.DelayFrame(1, cancellationToken: ct);
    }
}

public sealed class MenuState : GameState
{
}
```

## Example 2: state with sub-states (marker interface)

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using Piggy.Code.StateMachine;

public sealed class GameplayState : GameState
{
    public GameplayState()
    {
        SubStateMachine.Register(new GameplayIdleState());
        SubStateMachine.Register(new GameplayCombatState());
    }

    protected override UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken ct)
        where T : struct, IGameStateContext
    {
        return SubStateMachine.ChangeStateAsync<GameplayIdleState, T>(gameStateContext, cancellationToken: ct);
    }
}

public sealed class GameplayIdleState : GameSubState
{
}

public sealed class GameplayCombatState : GameSubState
{
}
```

Switch sub-state using the same method (routing is internal):

```csharp
await stateMachine.ChangeStateAsync<GameplayCombatState, SceneContext>(context, cancellationToken: ct);
```

## Notes

- `EnterAsync/ExitAsync` are internal; override `OnEnterAsync/OnExitAsync` in your states.
- Any state can have sub-states (every `GameState` has `SubStateMachine`).
- Sub-states are identified by `ISubState` (via `GameSubState`) and must be registered in the parent state's `SubStateMachine`.
- Do not call `ChangeStateAsync` on the same `StateMachine` inside its own `OnEnterAsync/OnExitAsync`.
- To re-enter the same state on demand, use `force = true`:

```csharp
await stateMachine.ChangeStateAsync<MenuState, SceneContext>(context, force: true, cancellationToken: ct);
```
