using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationPathPresenter : ExplorationPathPresenterBase
{
    public ExplorationPathPresenter(ExplorationPathViewBase view, ExplorationPathModelBase model)
        : base(view, model)
    {
    }

    public override void ShowRoute(IReadOnlyList<Vector3> worldPoints)
    {
        model.SetRoute(worldPoints);
        view.ShowRoute(model.RoutePoints);
    }

    public override void HideRoute()
    {
        model.Clear();
        view.HideRoute();
    }

    protected override void OnInitialize()
    {
        HideRoute();
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        HideRoute();
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
