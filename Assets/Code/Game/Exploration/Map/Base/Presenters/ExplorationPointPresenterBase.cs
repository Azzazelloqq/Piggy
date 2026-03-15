using Code.Game.Async;
using MVP;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationPointPresenterBase
    : Presenter<ExplorationPointViewBase, ExplorationPointModelBase>
{
    protected ExplorationPointPresenterBase(ExplorationPointViewBase view, ExplorationPointModelBase model)
        : base(view, model)
    {
    }

    public abstract AsyncEvent<string> Selected { get; }
    public abstract string PointId { get; }
    public abstract void RefreshView();
    public abstract void SetSelected(bool isSelected);
    public abstract void SetMovementBlocked(bool isBlocked);
}
}
