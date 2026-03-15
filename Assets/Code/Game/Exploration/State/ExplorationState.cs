using System;
using System.Threading;
using Code.Game.Exploration.Authoring;
using Code.Game.Exploration.Domain;
using Code.Game.Exploration.Map;
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
    private ExplorationMapPresenter _mapPresenter;
    private ExplorationTimeOverlayView _timeOverlayView;

    protected override async UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
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
        await InitializeMapPresenterAsync(token);
        InitializeTimeOverlay();
        Debug.Log($"ExplorationState: entered location '{_session.WorldState.CurrentLocationId}'.");
    }

    protected override UniTask OnExitAsync(CancellationToken cancellationToken)
    {
        CleanupTimeOverlay();
        CleanupMapPresenter();
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
            session.TimeController);
        session.RuntimeServices.EventResolver.MarkTriggered(
            resolvedEvent,
            session.WorldState,
            session.CurrentLocationState);
        session.RefreshCurrentLocation();
    }

    private async UniTask InitializeMapPresenterAsync(CancellationToken token)
    {
        var mapView = ResolveRequiredMapView(_locationView);
        var locationAuthoring = ResolveRequiredLocationAuthoring(mapView);
        if (!LocationRuntimeBuilder.Validate(locationAuthoring, out var validationMessage))
        {
            throw new InvalidOperationException(validationMessage);
        }

        var runtimeDescriptor = LocationRuntimeBuilder.Build(locationAuthoring);
        var mapDefinition = ExplorationMapDefinitionBuilder.Build(_session, runtimeDescriptor);
        _mapPresenter = new ExplorationMapPresenter(
            mapView,
            new ExplorationMapModel(mapDefinition),
            _session);
        await _mapPresenter.InitializeAsync(token);
    }

    private void CleanupMapPresenter()
    {
        _mapPresenter?.Dispose();
        _mapPresenter = null;
    }

    private void InitializeTimeOverlay()
    {
        if (_locationView == null)
        {
            throw new InvalidOperationException("Location view must exist before initializing the time overlay.");
        }

        _timeOverlayView = _locationView.GetComponent<ExplorationTimeOverlayView>();
        if (_timeOverlayView == null)
        {
            _timeOverlayView = _locationView.gameObject.AddComponent<ExplorationTimeOverlayView>();
        }

        _timeOverlayView.Bind(_session.TimeService, _session.TimeController);
    }

    private void CleanupTimeOverlay()
    {
        _timeOverlayView?.Unbind();
        _timeOverlayView = null;
    }

    private static ExplorationMapView ResolveRequiredMapView(ExplorationLocationView locationView)
    {
        if (locationView == null)
        {
            throw new ArgumentNullException(nameof(locationView));
        }

        var mapView = locationView.GetComponentInChildren<ExplorationMapView>(true);
        if (mapView == null)
        {
            throw new InvalidOperationException(
                $"Location '{locationView.name}' does not contain an {nameof(ExplorationMapView)}.");
        }

        return mapView;
    }

    private static LocationAuthoring ResolveRequiredLocationAuthoring(ExplorationMapView mapView)
    {
        if (mapView == null)
        {
            throw new ArgumentNullException(nameof(mapView));
        }

        var locationAuthoring = mapView.LocationAuthoring != null
            ? mapView.LocationAuthoring
            : mapView.GetComponentInParent<LocationAuthoring>();
        if (locationAuthoring == null)
        {
            throw new InvalidOperationException(
                $"Map view '{mapView.name}' is missing a linked {nameof(LocationAuthoring)}.");
        }

        return locationAuthoring;
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
