using System;
using Code.Game.Bootstrap.State;
using Code.Game.MainMenu.States;
using Disposable;
using Piggy.Code.StateMachine;
using UnityEngine;

namespace Code.Game.Root
{
public class GameRoot : MonoBehaviourDisposable
{
    [SerializeField]
    private RootContext _rootContext;

    private readonly StateMachine _stateMachine = new();

    private async void Start()
    {
        var bootstrapState = new BootstrapState();
        _stateMachine.Register(bootstrapState);
        _stateMachine.Register(new MainMenuState());

        try
        {
            await _stateMachine.ChangeStateAsync<BootstrapState, BootstrapStateContext>(
                new BootstrapStateContext(_rootContext.UIContext),
                cancellationToken: destroyCancellationToken);

            var mainMenuStateContext = new MainMenuStateContext(_rootContext.UIContext);
            await _stateMachine.ChangeStateAsync<MainMenuState, MainMenuStateContext>(
                mainMenuStateContext,
                cancellationToken: destroyCancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
}