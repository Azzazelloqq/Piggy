using Code.Game.Exploration.Runtime;
using Code.Game.Root;
using Piggy.Code.StateMachine;

namespace Code.Game.Exploration.State
{
public readonly struct ExplorationStateContext : IGameStateContext
{
    public ExplorationStateContext(UIContext uiContext, ExplorationSession session)
    {
        UIContext = uiContext;
        Session = session;
    }

    public UIContext UIContext { get; }
    public ExplorationSession Session { get; }
}
}
