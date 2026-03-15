using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Exploration.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationMapPresenter : ExplorationMapPresenterBase
{
    private const string MovementFlowSourceId = "exploration_map_movement";
    private const int SelectionTimeCostUnits = 1;

    private readonly ExplorationSession _session;
    private readonly List<ExplorationPointPresenter> _pointPresenters = new();
    private readonly ExplorationActorPresenter _actorPresenter;
    private readonly ExplorationPathPresenter _pathPresenter;

    public ExplorationMapPresenter(
        ExplorationMapViewBase view,
        ExplorationMapModelBase model,
        ExplorationSession session)
        : base(view, model)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        var actorModel = new ExplorationActorModel();
        actorModel.SetWorldPosition(model.CurrentWorldPosition);
        _actorPresenter = new ExplorationActorPresenter(view.ActorView, actorModel);
        _pathPresenter = new ExplorationPathPresenter(view.PathView, new ExplorationPathModel());

        foreach (var pointView in view.PointViews ?? Array.Empty<ExplorationPointView>())
        {
            if (pointView == null || string.IsNullOrWhiteSpace(pointView.EntityId))
            {
                continue;
            }

            if (!model.Points.TryGetValue(pointView.EntityId, out var pointData))
            {
                pointView.gameObject.SetActive(false);
                continue;
            }

            var pointModel = new ExplorationPointModel();
            pointModel.Configure(pointData);
            _pointPresenters.Add(new ExplorationPointPresenter(pointView, pointModel));
        }
    }

    protected override void OnInitialize()
    {
        SubscribeToPoints();
        InitializeChildren();
        SyncVisualState();
    }

    protected override async ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeToPoints();
        await InitializeChildrenAsync(token);
        SyncVisualState();
    }

    protected override void OnDispose()
    {
        UnsubscribeFromPoints();
        DisposeChildren();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeFromPoints();
        DisposeChildren();
        return default;
    }

    private void InitializeChildren()
    {
        _actorPresenter.Initialize();
        _pathPresenter.Initialize();
        foreach (var pointPresenter in _pointPresenters)
        {
            pointPresenter.Initialize();
        }
    }

    private async UniTask InitializeChildrenAsync(CancellationToken token)
    {
        await _actorPresenter.InitializeAsync(token);
        await _pathPresenter.InitializeAsync(token);
        foreach (var pointPresenter in _pointPresenters)
        {
            await pointPresenter.InitializeAsync(token);
        }
    }

    private void DisposeChildren()
    {
        foreach (var pointPresenter in _pointPresenters)
        {
            pointPresenter.Dispose();
        }

        _pathPresenter.Dispose();
        _actorPresenter.Dispose();
    }

    private void SubscribeToPoints()
    {
        foreach (var pointPresenter in _pointPresenters)
        {
            pointPresenter.Selected.Subscribe(HandlePointSelected);
        }
    }

    private void UnsubscribeFromPoints()
    {
        foreach (var pointPresenter in _pointPresenters)
        {
            pointPresenter.Selected.Unsubscribe(HandlePointSelected);
        }
    }

    private void SyncVisualState()
    {
        _actorPresenter.SetWorldPosition(model.CurrentWorldPosition);
        _pathPresenter.HideRoute();
        UpdatePointState();
    }

    private void UpdatePointState()
    {
        foreach (var pointPresenter in _pointPresenters)
        {
            pointPresenter.SetSelected(string.Equals(pointPresenter.PointId, model.SelectedPointId, StringComparison.Ordinal));
            pointPresenter.SetMovementBlocked(model.IsMoving);
            pointPresenter.RefreshView();
        }
    }

    private async UniTask HandlePointSelected(string pointId)
    {
        if (!model.TryPlanMovement(pointId, out var routePlan))
        {
            return;
        }

        _session.TimeController.SpendUnits(SelectionTimeCostUnits);
        UpdatePointState();
        _pathPresenter.ShowRoute(routePlan.Waypoints);

        if (!routePlan.RequiresMovement)
        {
            CompleteMovement(routePlan.PointId, routePlan.TargetNodeId);
            UpdatePointState();
            return;
        }

        model.BeginMovement();
        UpdatePointState();

        var flowUnitsPerSecond = Mathf.Max(0.01f, _session.DefaultFlowUnitsPerSecond);
        _session.TimeController.BeginFlow(MovementFlowSourceId, flowUnitsPerSecond);

        try
        {
            await _actorPresenter.MoveAlongAsync(routePlan.Waypoints, view.MovementSpeed, disposeToken);
            CompleteMovement(routePlan.PointId, routePlan.TargetNodeId);
        }
        catch
        {
            model.CancelMovement();
            throw;
        }
        finally
        {
            _session.TimeController.EndFlow(MovementFlowSourceId);
            UpdatePointState();
        }
    }

    private void CompleteMovement(string pointId, string targetNodeId)
    {
        model.CompleteMovement(pointId);
        _actorPresenter.SetWorldPosition(model.CurrentWorldPosition);

        _session.WorldState.CurrentNodeId = targetNodeId;
        if (_session.CurrentLocationState.Nodes.TryGetValue(targetNodeId, out var nodeState))
        {
            nodeState.IsVisited = true;
        }

        _session.RefreshCurrentLocation();
    }
}
}
