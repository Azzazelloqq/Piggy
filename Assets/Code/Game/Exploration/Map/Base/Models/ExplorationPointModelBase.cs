using MVP;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationPointModelBase : Model
{
    public abstract string PointId { get; }
    public abstract ExplorationMapPointData Data { get; }
    public abstract bool IsVisible { get; }
    public abstract bool IsInteractable { get; }
    public abstract bool IsSelected { get; }
    public abstract void Configure(ExplorationMapPointData data);
    public abstract bool CanRequestSelection();
    public abstract void SetSelected(bool isSelected);
    public abstract void SetMovementBlocked(bool isBlocked);
}
}
