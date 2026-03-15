using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationActorModel : ExplorationActorModelBase
{
    private Vector3 _worldPosition;
    private bool _isMoving;

    public override Vector3 WorldPosition => _worldPosition;
    public override bool IsMoving => _isMoving;

    public override void SetWorldPosition(Vector3 worldPosition)
    {
        _worldPosition = worldPosition;
    }

    public override void BeginMovement()
    {
        _isMoving = true;
    }

    public override void CompleteMovement(Vector3 worldPosition)
    {
        _isMoving = false;
        _worldPosition = worldPosition;
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
