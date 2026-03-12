using System;
using Code.Game.Exploration.Runtime;
using Code.Game.Exploration.View;
using Code.Game.Root;
using Piggy.Code.StateMachine;

namespace Code.Game.Exploration.State
{
public readonly struct ExplorationStateContext : IGameStateContext
{
    public ExplorationStateContext(
        UIContext uiContext,
        ExplorationSession session,
        ExplorationLocationView locationView)
    {
        UIContext = uiContext ?? throw new ArgumentNullException(nameof(uiContext));
        Session = session ?? throw new ArgumentNullException(nameof(session));
        LocationView = locationView ?? throw new ArgumentNullException(nameof(locationView));
    }

    public UIContext UIContext { get; }
    public ExplorationSession Session { get; }
    public ExplorationLocationView LocationView { get; }
}
}
