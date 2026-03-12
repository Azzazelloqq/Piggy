using System;
using System.Threading;
using Code.Game.Exploration.Domain;
using Code.Game.Exploration.Runtime;
using Code.Game.Exploration.View;
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
    private ExplorationLocationView _locationView;

    protected override UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
    {
        if (gameStateContext is not ExplorationStateContext context)
        {
            throw new ArgumentException(
                $"Expected {nameof(ExplorationStateContext)} but received {typeof(T).Name}.",
                nameof(gameStateContext));
        }

        _session = context.Session;
        _locationView = context.LocationView;
        ValidateLoadedLocation(_session, _locationView);
        RunEnterEvent(_session);
        Debug.Log($"ExplorationState: entered location '{_session.WorldState.CurrentLocationId}'.");

        return UniTask.CompletedTask;
    }

    protected override UniTask OnExitAsync(CancellationToken cancellationToken)
    {
        CleanupView(_locationView);
        _locationView = null;

        _session = null;
        return UniTask.CompletedTask;
    }

    private static void ValidateLoadedLocation(ExplorationSession session, ExplorationLocationView locationView)
    {
        if (!string.IsNullOrWhiteSpace(locationView.LocationId) &&
            !string.Equals(locationView.LocationId, session.CurrentLocationConfig.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Loaded location view '{locationView.LocationId}' does not match the active location '{session.CurrentLocationConfig.Id}'.");
        }
    }

    private static void RunEnterEvent(ExplorationSession session)
    {
        if (!session.RuntimeServices.EventResolver.TryResolve(
                EventTriggerType.Enter,
                session.CurrentLocationConfig.Events,
                session.WorldState,
                session.CurrentLocationState,
                session.WorldState.CurrentNodeId,
                string.Empty,
                out var resolvedEvent))
        {
            return;
        }

        session.RuntimeServices.ActionExecutor.Execute(
            resolvedEvent.Actions,
            session.WorldState,
            session.CurrentLocationState,
            session.TimeService);
        session.RuntimeServices.EventResolver.MarkTriggered(
            resolvedEvent,
            session.WorldState,
            session.CurrentLocationState);
        session.RefreshCurrentLocation();
    }

    private static void CleanupView(Component view)
    {
        if (view != null)
        {
            Object.Destroy(view.gameObject);
        }
    }
}
}
