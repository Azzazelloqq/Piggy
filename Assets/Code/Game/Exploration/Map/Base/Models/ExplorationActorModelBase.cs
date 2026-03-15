using MVP;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationActorModelBase : Model
{
    public abstract Vector3 WorldPosition { get; }
    public abstract bool IsMoving { get; }
    public abstract void SetWorldPosition(Vector3 worldPosition);
    public abstract void BeginMovement();
    public abstract void CompleteMovement(Vector3 worldPosition);
}
}
