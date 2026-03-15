using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationPathModel : ExplorationPathModelBase
{
    private Vector3[] _routePoints = Array.Empty<Vector3>();
    private bool _isVisible;

    public override IReadOnlyList<Vector3> RoutePoints => _routePoints;
    public override bool IsVisible => _isVisible;

    public override void SetRoute(IReadOnlyList<Vector3> routePoints)
    {
        if (routePoints == null || routePoints.Count == 0)
        {
            Clear();
            return;
        }

        _routePoints = new Vector3[routePoints.Count];
        for (var i = 0; i < routePoints.Count; i++)
        {
            _routePoints[i] = routePoints[i];
        }

        _isVisible = true;
    }

    public override void Clear()
    {
        _routePoints = Array.Empty<Vector3>();
        _isVisible = false;
    }

    protected override void OnInitialize()
    {
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
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
