using System.Collections.Generic;
using MVP;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationMapModelBase : Model
{
    public abstract IReadOnlyDictionary<string, ExplorationMapPointData> Points { get; }
    public abstract string CurrentNodeId { get; }
    public abstract Vector3 CurrentWorldPosition { get; }
    public abstract string SelectedPointId { get; }
    public abstract bool IsMoving { get; }
    public abstract bool TryPlanMovement(string pointId, out ExplorationMapRoutePlan routePlan);
    public abstract void BeginMovement();
    public abstract void CancelMovement();
    public abstract void CompleteMovement(string pointId);
}
}
