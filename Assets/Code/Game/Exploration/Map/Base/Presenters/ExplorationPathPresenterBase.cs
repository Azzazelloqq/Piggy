using System.Collections.Generic;
using MVP;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationPathPresenterBase
    : Presenter<ExplorationPathViewBase, ExplorationPathModelBase>
{
    protected ExplorationPathPresenterBase(ExplorationPathViewBase view, ExplorationPathModelBase model)
        : base(view, model)
    {
    }

    public abstract void ShowRoute(IReadOnlyList<Vector3> worldPoints);
    public abstract void HideRoute();
}
}
