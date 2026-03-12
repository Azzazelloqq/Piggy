using Code.Game.Flow;
using Code.Game.MainMenu.Window;
using InGameLogger;
using LocalSaveSystem;
using Piggy.Code.StateMachine;

namespace Code.Game.MainMenu.States
{
public readonly struct MainMenuSubStateContext : IGameStateContext
{
    public MainMenuSubStateContext(
        MainMenuScreen screen,
        MainMenuPresenter presenter,
        IMainMenuNavigator navigator,
        IInGameLogger logger,
        ISaveStore saveStore,
        IGameFlowService gameFlowService)
    {
        Screen = screen;
        Presenter = presenter;
        Navigator = navigator;
        Logger = logger;
        SaveStore = saveStore;
        GameFlowService = gameFlowService;
    }

    public MainMenuScreen Screen { get; }
    public MainMenuPresenter Presenter { get; }
    public IMainMenuNavigator Navigator { get; }
    public IInGameLogger Logger { get; }
    public ISaveStore SaveStore { get; }
    public IGameFlowService GameFlowService { get; }
}
}