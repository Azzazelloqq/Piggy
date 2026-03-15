using MVP;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationMapPresenterBase : Presenter<ExplorationMapViewBase, ExplorationMapModelBase>
{
    protected ExplorationMapPresenterBase(ExplorationMapViewBase view, ExplorationMapModelBase model)
        : base(view, model)
    {
    }
}
}
