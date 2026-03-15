using System.Collections.Generic;
using Code.Game.Exploration.Authoring;
using MVP;

namespace Code.Game.Exploration.Map
{
public abstract class ExplorationMapViewBase : ViewMonoBehaviour<ExplorationMapPresenterBase>
{
    public abstract LocationAuthoring LocationAuthoring { get; }
    public abstract IReadOnlyList<ExplorationPointView> PointViews { get; }
    public abstract ExplorationActorView ActorView { get; }
    public abstract ExplorationPathView PathView { get; }
    public abstract float MovementSpeed { get; }
}
}
