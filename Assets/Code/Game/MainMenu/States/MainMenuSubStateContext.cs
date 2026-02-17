using Code.Game.MainMenu.Window;
using InGameLogger;
using Piggy.Code.StateMachine;

namespace Code.Game.MainMenu.States
{
public readonly struct MainMenuSubStateContext : IGameStateContext
{
    public MainMenuSubStateContext(
        MainMenuScreen screen,
        MainMenuPresenter presenter,
        IMainMenuNavigator navigator,
        IInGameLogger logger)
    {
        Screen = screen;
        Presenter = presenter;
        Navigator = navigator;
        Logger = logger;
    }

    public MainMenuScreen Screen { get; }
    public MainMenuPresenter Presenter { get; }
    public IMainMenuNavigator Navigator { get; }
    public IInGameLogger Logger { get; }
}
}