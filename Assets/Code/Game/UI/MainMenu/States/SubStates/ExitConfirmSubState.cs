using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Game.MainMenu.Window;
using Piggy.Code.StateMachine;
using UnityEngine;

namespace Code.Game.MainMenu.States
{
public sealed class ExitConfirmSubState : GameSubState
{
    private MainMenuPresenter _presenter;
    private IMainMenuNavigator _navigator;

    protected override UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
    {
        var context = (MainMenuSubStateContext)(object)gameStateContext;
        _presenter = context.Presenter;
        _navigator = context.Navigator;

        _presenter.ExitConfirmed.Subscribe(HandleExitConfirmed);
        _presenter.ExitCanceled.Subscribe(HandleExitCanceled);

        return UniTask.CompletedTask;
    }

    protected override UniTask OnExitAsync(CancellationToken cancellationToken)
    {
        _presenter.ExitConfirmed.Unsubscribe(HandleExitConfirmed);
        _presenter.ExitCanceled.Unsubscribe(HandleExitCanceled);

        return UniTask.CompletedTask;
    }

    private UniTask HandleExitConfirmed()
    {
        Application.Quit();
        return UniTask.CompletedTask;
    }

    private UniTask HandleExitCanceled()
    {
        return _navigator.NavigateAsync(MainMenuScreen.Menu);
    }
}
}