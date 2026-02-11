using Code.Game.Root;
using Piggy.Code.StateMachine;

namespace Code.Game.MainMenu.States
{
public struct MainMenuStateContext : IGameStateContext
{
    public UIContext UIContext { get; }

    public MainMenuStateContext(UIContext uiContext)
    {
        UIContext = uiContext;
    }
}
}