using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Game.MainMenu.Window;
using Piggy.Code.StateMachine;
using UnityEngine;

namespace Code.Game.MainMenu.States
{
public sealed class MenuSubState : GameSubState
{
    private MainMenuPresenter _presenter;
    private IMainMenuNavigator _navigator;

    protected override UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
    {
        var context = (MainMenuSubStateContext)(object)gameStateContext;
        _presenter = context.Presenter;
        _navigator = context.Navigator;

        _presenter.PlayRequested.Subscribe(HandlePlayRequested);
        _presenter.SettingsRequested.Subscribe(HandleSettingsRequested);
        _presenter.ExitRequested.Subscribe(HandleExitRequested);

        return UniTask.CompletedTask;
    }

    protected override UniTask OnExitAsync(CancellationToken cancellationToken)
    {
        _presenter.PlayRequested.Unsubscribe(HandlePlayRequested);
        _presenter.SettingsRequested.Unsubscribe(HandleSettingsRequested);
        _presenter.ExitRequested.Unsubscribe(HandleExitRequested);

        return UniTask.CompletedTask;
    }

    private UniTask HandlePlayRequested()
    {
        Debug.Log("MainMenuState: Play requested.");
        return UniTask.CompletedTask;
    }

    private UniTask HandleSettingsRequested()
    {
        return _navigator.NavigateAsync(MainMenuScreen.Settings);
    }

    private UniTask HandleExitRequested()
    {
        return _navigator.NavigateAsync(MainMenuScreen.ExitConfirm);
    }
}
}