using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public sealed partial class MainMenuPresenter : MainMenuPresenterBase
{
    private readonly MainMenuSettingsPresenter _settingsPresenter;
    private readonly MainMenuExitConfirmPresenter _exitPresenter;
    private readonly MainMenuViewBase.LayoutData _layoutSettings;
    private MainMenuScreen _currentScreen;
    private bool _isTransitioning;

    public event Action SettingsBackRequested;
    public event Action SettingsApplyRequested;
    public event Action ExitConfirmed;
    public event Action ExitCanceled;

    public MainMenuPresenter(MainMenuViewBase view, MainMenuModelBase model)
        : base(view, model)
    {
        var settingsViewBase = view.SettingsView;
        var exitConfirmViewBase = view.ExitConfirmView;

        var settingsModel = new MainMenuSettingsModel();
        var exitModel = new MainMenuExitConfirmModel();

        _settingsPresenter = new MainMenuSettingsPresenter(settingsViewBase, settingsModel);
        _exitPresenter = new MainMenuExitConfirmPresenter(exitConfirmViewBase, exitModel);

        compositeDisposable.AddDisposable(_settingsPresenter);
        compositeDisposable.AddDisposable(_exitPresenter);

        _layoutSettings = view.Layout;
    }

    public void ApplyScreenLayoutImmediate(MainMenuScreen screen)
    {
        var screenLayout = GetScreenLayout(screen);

        ApplyPanelImmediate(
            view,
            this,
            _layoutSettings.GetMenuPosition(screenLayout.Menu),
            screenLayout.Menu == MainMenuPosition.Shown);

        ApplyPanelImmediate(
            view.SettingsView,
            _settingsPresenter,
            _layoutSettings.GetSettingsPosition(screenLayout.Settings),
            screenLayout.Settings == MainMenuSettingsPosition.Shown);

        ApplyPanelImmediate(
            view.ExitConfirmView,
            _exitPresenter,
            _layoutSettings.GetExitPosition(screenLayout.Exit),
            screenLayout.Exit == MainMenuExitConfirmPosition.Shown);

        _currentScreen = screen;
    }

    public async UniTask<bool> TryTransitionToScreenAsync(MainMenuScreen targetScreen, CancellationToken token)
    {
        if (_isTransitioning || _currentScreen == targetScreen)
        {
            return false;
        }

        _isTransitioning = true;
        try
        {
            var screenLayout = GetScreenLayout(targetScreen);
            await UniTask.WhenAll(
                MovePanelAsync(
                    view,
                    this,
                    _layoutSettings.GetMenuPosition(screenLayout.Menu),
                    screenLayout.Menu == MainMenuPosition.Shown,
                    token),
                MovePanelAsync(
                    view.SettingsView,
                    _settingsPresenter,
                    _layoutSettings.GetSettingsPosition(screenLayout.Settings),
                    screenLayout.Settings == MainMenuSettingsPosition.Shown,
                    token),
                MovePanelAsync(
                    view.ExitConfirmView,
                    _exitPresenter,
                    _layoutSettings.GetExitPosition(screenLayout.Exit),
                    screenLayout.Exit == MainMenuExitConfirmPosition.Shown,
                    token));

            _currentScreen = targetScreen;
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    protected override void OnInitialize()
    {
        SubscribeOnEvents();

        _settingsPresenter.Initialize();
        _exitPresenter.Initialize();

        view.SetVisible(model.IsVisible);
    }

    protected override async ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeOnEvents();

        await _settingsPresenter.InitializeAsync(token);
        await _exitPresenter.InitializeAsync(token);

        view.SetVisible(model.IsVisible);
    }

    protected override void OnDispose()
    {
        UnsubscribeOneEvents();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeOneEvents();

        return default;
    }

    public override void Show()
    {
        model.Show();
    }

    public override void Hide()
    {
        model.Hide();
    }

    public override void RequestPlay()
    {
        model.RequestPlay();
    }

    public override void RequestSettings()
    {
        model.RequestSettings();
    }

    public override void RequestExit()
    {
        model.RequestExit();
    }

    private void HandlePlayClicked()
    {
        model.RequestPlay();
    }

    private void HandleSettingsClicked()
    {
        model.RequestSettings();
    }

    private void HandleExitClicked()
    {
        model.RequestExit();
    }

    private void HandleVisibilityChanged(bool isVisible)
    {
        view.SetVisible(isVisible);
    }

    private void HandlePlayRequested()
    {
        NotifyPlayRequested();
    }

    private void HandleSettingsRequested()
    {
        NotifySettingsRequested();
    }

    private void HandleExitRequested()
    {
        NotifyExitRequested();
    }

    private void HandleSettingsBackRequested()
    {
        SettingsBackRequested?.Invoke();
    }

    private void HandleSettingsApplyRequested()
    {
        SettingsApplyRequested?.Invoke();
    }

    private void HandleExitConfirmed()
    {
        ExitConfirmed?.Invoke();
    }

    private void HandleExitCanceled()
    {
        ExitCanceled?.Invoke();
    }

    private void SubscribeOnEvents()
    {
        view.PlayClicked += HandlePlayClicked;
        view.SettingsClicked += HandleSettingsClicked;
        view.ExitClicked += HandleExitClicked;

        model.VisibilityChanged += HandleVisibilityChanged;
        model.PlayRequested += HandlePlayRequested;
        model.SettingsRequested += HandleSettingsRequested;
        model.ExitRequested += HandleExitRequested;

        _settingsPresenter.BackRequested += HandleSettingsBackRequested;
        _settingsPresenter.ApplyRequested += HandleSettingsApplyRequested;

        _exitPresenter.Confirmed += HandleExitConfirmed;
        _exitPresenter.Canceled += HandleExitCanceled;
    }

    private void UnsubscribeOneEvents()
    {
        view.PlayClicked -= HandlePlayClicked;
        view.SettingsClicked -= HandleSettingsClicked;
        view.ExitClicked -= HandleExitClicked;

        model.VisibilityChanged -= HandleVisibilityChanged;
        model.PlayRequested -= HandlePlayRequested;
        model.SettingsRequested -= HandleSettingsRequested;
        model.ExitRequested -= HandleExitRequested;

        _settingsPresenter.BackRequested -= HandleSettingsBackRequested;
        _settingsPresenter.ApplyRequested -= HandleSettingsApplyRequested;

        _exitPresenter.Confirmed -= HandleExitConfirmed;
        _exitPresenter.Canceled -= HandleExitCanceled;
    }

    private static MainMenuScreenLayout GetScreenLayout(MainMenuScreen screen)
    {
        switch (screen)
        {
            case MainMenuScreen.Menu:
                return new MainMenuScreenLayout(MainMenuPosition.Shown, MainMenuSettingsPosition.HiddenLeft,
                    MainMenuExitConfirmPosition.HiddenBottom);
            case MainMenuScreen.Settings:
                return new MainMenuScreenLayout(MainMenuPosition.HiddenRight, MainMenuSettingsPosition.Shown,
                    MainMenuExitConfirmPosition.HiddenBottom);
            case MainMenuScreen.ExitConfirm:
                return new MainMenuScreenLayout(MainMenuPosition.HiddenUp, MainMenuSettingsPosition.HiddenLeft,
                    MainMenuExitConfirmPosition.Shown);
            default:
                return new MainMenuScreenLayout(MainMenuPosition.Shown, MainMenuSettingsPosition.HiddenLeft,
                    MainMenuExitConfirmPosition.HiddenBottom);
        }
    }

    private void ApplyPanelImmediate(
        IMainMenuPanelView panelView,
        IMainMenuPanelPresenter presenter,
        Vector2 position,
        bool show)
    {
        MainMenuPanelAnimator.SetImmediate(panelView.Panel, position);
        if (show)
        {
            presenter.Show();
            panelView.SetInteractable(true);
        }
        else
        {
            panelView.SetInteractable(false);
            presenter.Hide();
        }
    }

    private async UniTask MovePanelAsync(
        IMainMenuPanelView panelView,
        IMainMenuPanelPresenter presenter,
        Vector2 position,
        bool show,
        CancellationToken token)
    {
        var ease = show ? _layoutSettings.ShowEase : _layoutSettings.HideEase;
        var duration = _layoutSettings.TransitionDuration;
        var useUnscaledTime = _layoutSettings.UseUnscaledTime;

        if (show)
        {
            presenter.Show();
        }

        panelView.SetInteractable(false);

        await MainMenuPanelAnimator.MoveAsync(
            panelView.Panel,
            position,
            duration,
            ease,
            useUnscaledTime,
            token);

        if (show)
        {
            panelView.SetInteractable(true);
        }
        else
        {
            presenter.Hide();
        }
    }
}
}