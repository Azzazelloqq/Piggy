using Code.Game.Root;
using Piggy.Code.StateMachine;

namespace Code.Game.Bootstrap.State
{
public struct BootstrapStateContext : IGameStateContext
{
    public UIContext UIContext { get; }

    public BootstrapStateContext(UIContext uiContext)
    {
        UIContext = uiContext;
    }
}
}