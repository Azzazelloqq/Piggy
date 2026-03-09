using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Game.MainMenu.Window;
using Code.Generated.Addressables;
using InGameLogger;
using LightDI.Runtime;
using LocalSaveSystem;
using Piggy.Code.StateMachine;
using ResourceLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.MainMenu.States
{
public sealed class MainMenuState : GameState, IMainMenuNavigator
{
    private readonly IInGameLogger _logger;
    private readonly ISaveStore _saveStore;
    private readonly IResourceLoader _resourceLoader;
    private MainMenuPresenter _menuPresenter;

    private MainMenuViewBase _menuView;

    private CancellationToken _stateToken;

    public MainMenuState([Inject] IInGameLogger logger, [Inject] ISaveStore saveStore, [Inject] IResourceLoader resourceLoader)
    {
        _logger = logger;
        _saveStore = saveStore;
        _resourceLoader = resourceLoader;
        SubStateMachine.Register(new MenuSubState());
        SubStateMachine.Register(new SettingsSubState());
        SubStateMachine.Register(new ExitConfirmSubState());
        SubStateMachine.Register(new SavesSubState());
        SubStateMachine.Register(new CharacterCreationSubState());
    }

    protected override async UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
    {
        await base.OnEnterAsync(gameStateContext, token);

        if (gameStateContext is not MainMenuStateContext mainMenuStateContext)
        {
            return;
        }

        _stateToken = token;
        var uiContext = mainMenuStateContext.UIContext;
        var mainParent = uiContext.OverlaysParent;
        var mainMenuViewResourceId = ResourceIdsContainer.UIMainMenu.MainMenuView;
        _menuView = await _resourceLoader.LoadAndCreateAsync<MainMenuViewBase, Transform>(mainMenuViewResourceId, mainParent, token);

        var menuModel = new MainMenuModel();
        _menuPresenter = MainMenuPresenterFactory.CreateMainMenuPresenter(_menuView, menuModel);

        await _menuPresenter.InitializeAsync(token);

        _menuPresenter.ApplyScreenLayoutImmediate(MainMenuScreen.Menu);
        await ChangeSubStateAsync(MainMenuScreen.Menu);
    }

    protected override UniTask OnExitAsync(CancellationToken ct)
    {
        CleanupImmediate();
        return UniTask.CompletedTask;
    }

    public UniTask NavigateAsync(MainMenuScreen targetScreen)
    {
        return TransitionToScreenAsync(targetScreen);
    }

    private async UniTask TransitionToScreenAsync(MainMenuScreen targetScreen)
    {
        try
        {
            if (await _menuPresenter.TryTransitionToScreenAsync(targetScreen, _stateToken))
            {
                await ChangeSubStateAsync(targetScreen);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger.LogException(exception);
        }
    }

    private void CleanupImmediate()
    {
        _menuPresenter.Dispose();
        _menuPresenter = null;

        _menuView = null;
    }

    private UniTask ChangeSubStateAsync(MainMenuScreen screen)
    {
        switch (screen)
        {
            case MainMenuScreen.Menu:
                return SubStateMachine.ChangeStateAsync<MenuSubState, MainMenuSubStateContext>(
                    BuildSubStateContext(screen), cancellationToken: _stateToken);
            case MainMenuScreen.Settings:
                return SubStateMachine.ChangeStateAsync<SettingsSubState, MainMenuSubStateContext>(
                    BuildSubStateContext(screen), cancellationToken: _stateToken);
            case MainMenuScreen.ExitConfirm:
                return SubStateMachine.ChangeStateAsync<ExitConfirmSubState, MainMenuSubStateContext>(
                    BuildSubStateContext(screen), cancellationToken: _stateToken);
            case MainMenuScreen.Saves:
                return SubStateMachine.ChangeStateAsync<SavesSubState, MainMenuSubStateContext>(
                    BuildSubStateContext(screen), cancellationToken: _stateToken);
            case MainMenuScreen.CharacterCreation:
                return SubStateMachine.ChangeStateAsync<CharacterCreationSubState, MainMenuSubStateContext>(
                    BuildSubStateContext(screen), cancellationToken: _stateToken);
            default:
                return SubStateMachine.ChangeStateAsync<MenuSubState, MainMenuSubStateContext>(
                    BuildSubStateContext(MainMenuScreen.Menu), cancellationToken: _stateToken);
        }
    }

    private MainMenuSubStateContext BuildSubStateContext(MainMenuScreen screen)
    {
        return new MainMenuSubStateContext(
            screen,
            _menuPresenter,
            this,
            _logger,
            _saveStore);
    }
}
}