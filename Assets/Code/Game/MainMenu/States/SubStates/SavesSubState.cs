using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Game.MainMenu.Window;
using Piggy.Code.StateMachine;
using UnityEngine;

namespace Code.Game.MainMenu.States
{
public sealed class SavesSubState : GameSubState
{
    private MainMenuPresenter _presenter;
    private IMainMenuNavigator _navigator;

    protected override UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
    {
        var context = (MainMenuSubStateContext)(object)gameStateContext;
        _presenter = context.Presenter;
        _navigator = context.Navigator;

        _presenter.SavesBackRequested.Subscribe(HandleBackRequested);
        _presenter.SaveSlotSelected.Subscribe(HandleSlotSelected);

        return UniTask.CompletedTask;
    }

    protected override UniTask OnExitAsync(CancellationToken cancellationToken)
    {
        _presenter.SavesBackRequested.Unsubscribe(HandleBackRequested);
        _presenter.SaveSlotSelected.Unsubscribe(HandleSlotSelected);

        return UniTask.CompletedTask;
    }

    private UniTask HandleBackRequested()
    {
        return _navigator.NavigateAsync(MainMenuScreen.Menu);
    }

    private UniTask HandleSlotSelected(GameProfilesSlotData slot)
    {
        if (slot.HasSave)
        {
            Debug.Log($"MainMenuState: load save slot {slot.Index} requested.");
        }
        else
        {
            Debug.Log($"MainMenuState: create character for slot {slot.Index} requested.");
        }

        return UniTask.CompletedTask;
    }
}
}
