using System.Threading;
using Code.Game.Exploration.Runtime;
using Code.Game.Root;
using Cysharp.Threading.Tasks;
using Piggy.Code.StateMachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.Exploration.State
{
public sealed class ExplorationState : GameState
{
    private ExplorationSession _session;
    private GameObject _root;

    protected override UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
    {
        if (gameStateContext is not ExplorationStateContext context)
        {
            return UniTask.CompletedTask;
        }

        _session = context.Session;
        _root = CreateRoot(context.UIContext);

        if (_session != null)
        {
            Debug.Log($"ExplorationState: entered location '{_session.WorldState.CurrentLocationId}'.");
        }

        return UniTask.CompletedTask;
    }

    protected override UniTask OnExitAsync(CancellationToken cancellationToken)
    {
        if (_root != null)
        {
            Object.Destroy(_root);
            _root = null;
        }

        _session = null;
        return UniTask.CompletedTask;
    }

    private static GameObject CreateRoot(UIContext uiContext)
    {
        var root = new GameObject("[ExplorationState]");
        if (uiContext != null && uiContext.DynamicObjectsParent != null)
        {
            root.transform.SetParent(uiContext.DynamicObjectsParent, false);
        }

        return root;
    }
}
}
