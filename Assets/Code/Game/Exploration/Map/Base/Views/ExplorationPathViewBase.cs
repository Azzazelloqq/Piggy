using System.Collections.Generic;
using MVP;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationPathViewBase : ViewMonoBehaviour<ExplorationPathPresenterBase>
{
    public abstract void ShowRoute(IReadOnlyList<Vector3> worldPoints);
    public abstract void HideRoute();
}
}
