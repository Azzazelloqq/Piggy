using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MVP;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationActorViewBase : ViewMonoBehaviour<ExplorationActorPresenterBase>
{
    public abstract void SetWorldPosition(Vector3 worldPosition);
    public abstract UniTask MoveAlongAsync(IReadOnlyList<Vector3> waypoints, float speed, CancellationToken token);
}
}
