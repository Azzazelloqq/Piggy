using Code.Game.Root;
using Piggy.Code.StateMachine;

namespace Code.Game.MainMenu.States
{
public struct MainMenuStateContext : IGameStateContext
{
    public UIContext UIContext { get; }
    public string MainMenuResourceId { get; }

    public MainMenuStateContext(UIContext uiContext, string mainMenuResourceId)
    {
        UIContext = uiContext;
        MainMenuResourceId = mainMenuResourceId;
    }
}
}