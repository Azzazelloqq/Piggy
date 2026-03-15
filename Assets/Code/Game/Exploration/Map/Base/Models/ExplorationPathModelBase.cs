using System.Collections.Generic;
using MVP;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationPathModelBase : Model
{
    public abstract IReadOnlyList<Vector3> RoutePoints { get; }
    public abstract bool IsVisible { get; }
    public abstract void SetRoute(IReadOnlyList<Vector3> routePoints);
    public abstract void Clear();
}
}
