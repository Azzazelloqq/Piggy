using System.Threading;
using System.Threading.Tasks;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationPointModel : ExplorationPointModelBase
{
    private ExplorationMapPointData _data;
    private bool _isMovementBlocked;
    private bool _isSelected;

    public override string PointId => _data.EntityId;
    public override ExplorationMapPointData Data => _data;
    public override bool IsVisible => _data.IsVisible;
    public override bool IsInteractable => _data.IsInteractable && !_isMovementBlocked;
    public override bool IsSelected => _isSelected;

    public override void Configure(ExplorationMapPointData data)
    {
        _data = data;
        _isSelected = false;
        _isMovementBlocked = false;
    }

    public override bool CanRequestSelection()
    {
        return IsVisible && IsInteractable;
    }

    public override void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
    }

    public override void SetMovementBlocked(bool isBlocked)
    {
        _isMovementBlocked = isBlocked;
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
