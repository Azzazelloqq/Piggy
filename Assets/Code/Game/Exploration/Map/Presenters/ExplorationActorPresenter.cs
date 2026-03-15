using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationActorPresenter : ExplorationActorPresenterBase
{
    public ExplorationActorPresenter(ExplorationActorViewBase view, ExplorationActorModelBase model)
        : base(view, model)
    {
    }

    public override void SetWorldPosition(Vector3 worldPosition)
    {
        model.SetWorldPosition(worldPosition);
        view.SetWorldPosition(worldPosition);
    }

    public override async UniTask MoveAlongAsync(IReadOnlyList<Vector3> waypoints, float speed, CancellationToken token)
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            return;
        }

        model.SetWorldPosition(waypoints[0]);
        model.BeginMovement();
        view.SetWorldPosition(waypoints[0]);

        await view.MoveAlongAsync(waypoints, speed, token);
        model.CompleteMovement(waypoints[^1]);
    }

    protected override void OnInitialize()
    {
        view.SetWorldPosition(model.WorldPosition);
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        view.SetWorldPosition(model.WorldPosition);
        return default;
    }

    protected override void OnDispose()
    {
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        return default;
    }
}
}
