using Code.Game.Async;
using Code.Game.Exploration.Domain;
using MVP;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationPointViewBase : ViewMonoBehaviour<ExplorationPointPresenterBase>
{
    public abstract AsyncEvent Clicked { get; }
    public abstract string EntityId { get; }
    public abstract void SetDisplayName(string displayName);
    public abstract void SetEntityType(MapEntityType entityType);
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);
    public abstract void SetSelected(bool isSelected);
}
}
