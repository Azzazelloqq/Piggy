using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Game.MainMenu.Window;
using Piggy.Code.StateMachine;
using UnityEngine;

namespace Code.Game.MainMenu.States
{
public sealed class SettingsSubState : GameSubState
{
    private MainMenuPresenter _presenter;
    private IMainMenuNavigator _navigator;

    protected override UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
    {
        var context = (MainMenuSubStateContext)(object)gameStateContext;
        _presenter = context.Presenter;
        _navigator = context.Navigator;

        _presenter.SettingsBackRequested.Subscribe(HandleSettingsBackRequested);
        _presenter.SettingsApplyRequested.Subscribe(HandleSettingsApplyRequested);

        return UniTask.CompletedTask;
    }

    protected override UniTask OnExitAsync(CancellationToken cancellationToken)
    {
        _presenter.SettingsBackRequested.Unsubscribe(HandleSettingsBackRequested);
        _presenter.SettingsApplyRequested.Unsubscribe(HandleSettingsApplyRequested);

        return UniTask.CompletedTask;
    }

    private UniTask HandleSettingsBackRequested()
    {
        return _navigator.NavigateAsync(MainMenuScreen.Menu);
    }

    private UniTask HandleSettingsApplyRequested()
    {
        Debug.Log("MainMenuState: Settings apply requested.");
        return UniTask.CompletedTask;
    }
}
}