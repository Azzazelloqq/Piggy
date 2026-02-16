using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Game.MainMenu.Window;
using Piggy.Code.StateMachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.MainMenu.States
{
    public sealed class MainMenuState : GameState
    {
        private MainMenuPresenter _menuPresenter;

        private MainMenuViewBase _menuView;

        private CancellationToken _stateToken;

        public MainMenuState()
        {
            SubStateMachine.Register(new MenuSubState());
            SubStateMachine.Register(new SettingsSubState());
            SubStateMachine.Register(new ExitConfirmSubState());
        }

        protected override async UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
        {
            await base.OnEnterAsync(gameStateContext, token);

            var context = (MainMenuStateContext)(object)gameStateContext;

            _stateToken = token;
            var uiContext = context.UIContext;
            var mainParent = uiContext.MainUIParent;
            _menuView = Object.Instantiate(uiContext.MainMenuPrefab, mainParent, false);

            var menuModel = new MainMenuModel();
            _menuPresenter = new MainMenuPresenter(_menuView, menuModel);

            await _menuPresenter.InitializeAsync(token);
   
            SubscribeToPresenterEvents();

            _menuPresenter.ApplyScreenLayoutImmediate(MainMenuScreen.Menu);
            await ChangeSubStateAsync(MainMenuScreen.Menu, token);
        }

        protected override UniTask OnExitAsync(CancellationToken ct)
        {
            CleanupImmediate();
            return UniTask.CompletedTask;
        }

        private void SubscribeToPresenterEvents()
        {
            _menuPresenter.PlayRequested += HandlePlayRequested;
            _menuPresenter.SettingsRequested += HandleSettingsRequested;
            _menuPresenter.ExitRequested += HandleExitRequested;

            _menuPresenter.SettingsBackRequested += HandleSettingsBackRequested;
            _menuPresenter.SettingsApplyRequested += HandleSettingsApplyRequested;

            _menuPresenter.ExitConfirmed += HandleExitConfirmed;
            _menuPresenter.ExitCanceled += HandleExitCanceled;
        }

        private void UnsubscribeFromPresenterEvents()
        {
            _menuPresenter.PlayRequested -= HandlePlayRequested;
            _menuPresenter.SettingsRequested -= HandleSettingsRequested;
            _menuPresenter.ExitRequested -= HandleExitRequested;

            _menuPresenter.SettingsBackRequested -= HandleSettingsBackRequested;
            _menuPresenter.SettingsApplyRequested -= HandleSettingsApplyRequested;

            _menuPresenter.ExitConfirmed -= HandleExitConfirmed;
            _menuPresenter.ExitCanceled -= HandleExitCanceled;
        }

        private void HandlePlayRequested()
        {
            Debug.Log("MainMenuState: Play requested.");
        }

        private void HandleSettingsRequested()
        {
            TransitionToScreenAsync(MainMenuScreen.Settings).Forget();
        }

        private void HandleExitRequested()
        {
            TransitionToScreenAsync(MainMenuScreen.ExitConfirm).Forget();
        }

        private void HandleSettingsBackRequested()
        {
            TransitionToScreenAsync(MainMenuScreen.Menu).Forget();
        }

        private void HandleSettingsApplyRequested()
        {
            Debug.Log("MainMenuState: Settings apply requested.");
        }

        private void HandleExitConfirmed()
        {
            Application.Quit();
        }

        private void HandleExitCanceled()
        {
            TransitionToScreenAsync(MainMenuScreen.Menu).Forget();
        }

        private async UniTask TransitionToScreenAsync(MainMenuScreen targetScreen)
        {
            try
            {
                if (await _menuPresenter.TryTransitionToScreenAsync(targetScreen, _stateToken))
                {
                    await ChangeSubStateAsync(targetScreen, _stateToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CleanupImmediate()
        {
            UnsubscribeFromPresenterEvents();

            _menuPresenter.Dispose();
            _menuPresenter = null;

            _menuView = null;
        }

        private sealed class MenuSubState : GameSubState
        {
        }

        private sealed class SettingsSubState : GameSubState
        {
        }

        private sealed class ExitConfirmSubState : GameSubState
        {
        }

        private UniTask ChangeSubStateAsync(MainMenuScreen screen, CancellationToken token)
        {
            return screen switch
            {
                MainMenuScreen.Menu => SubStateMachine.ChangeStateAsync<MenuSubState, MainMenuSubStateContext>(
                    new MainMenuSubStateContext(screen),
                    cancellationToken: token),
                MainMenuScreen.Settings => SubStateMachine.ChangeStateAsync<SettingsSubState, MainMenuSubStateContext>(
                    new MainMenuSubStateContext(screen),
                    cancellationToken: token),
                MainMenuScreen.ExitConfirm => SubStateMachine.ChangeStateAsync<ExitConfirmSubState, MainMenuSubStateContext>(
                    new MainMenuSubStateContext(screen),
                    cancellationToken: token),
                _ => SubStateMachine.ChangeStateAsync<MenuSubState, MainMenuSubStateContext>(
                    new MainMenuSubStateContext(MainMenuScreen.Menu),
                    cancellationToken: token)
            };
        }

    }
}
