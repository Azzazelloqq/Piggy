using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuPresenter : MainMenuPresenterBase
{
    private readonly MainMenuSettingsPresenter _settingsPresenter;
    private readonly MainMenuExitConfirmPresenter _exitPresenter;
    private readonly MainMenuViewBase.LayoutData _layoutSettings;
    private readonly PanelHandle _menuPanel;
    private readonly PanelHandle _settingsPanel;
    private readonly PanelHandle _exitPanel;
    private Vector2 _menuShownPosition;
    private Vector2 _settingsShownPosition;
    private Vector2 _exitShownPosition;
    private bool _layoutCaptured;
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
        _menuPanel = new PanelHandle(view.Panel, Show, Hide, view.SetInteractable, view.AnimatedElements);
        _settingsPanel = new PanelHandle(settingsViewBase.Panel, _settingsPresenter.Show, _settingsPresenter.Hide,
            settingsViewBase.SetInteractable, settingsViewBase.AnimatedElements);
        _exitPanel = new PanelHandle(exitConfirmViewBase.Panel, _exitPresenter.Show, _exitPresenter.Hide,
            exitConfirmViewBase.SetInteractable, exitConfirmViewBase.AnimatedElements);
    }

    public void ApplyScreenLayoutImmediate(MainMenuScreen screen)
    {
        EnsureLayoutCaptured();

        var showMenu = screen == MainMenuScreen.Menu;
        var showSettings = screen == MainMenuScreen.Settings;
        var showExit = screen == MainMenuScreen.ExitConfirm;

        ApplyPanelImmediate(_menuPanel, GetMenuTarget(screen), showMenu);
        ApplyPanelImmediate(_settingsPanel, GetSettingsTarget(screen), showSettings);
        ApplyPanelImmediate(_exitPanel, GetExitTarget(screen), showExit);

        _currentScreen = screen;
    }

    public async UniTask<bool> TryTransitionToScreenAsync(MainMenuScreen targetScreen, CancellationToken token)
    {
        EnsureLayoutCaptured();

        if (_isTransitioning || _currentScreen == targetScreen)
        {
            return false;
        }

        _isTransitioning = true;
        try
        {
            var currentScreen = _currentScreen;

            await MovePanelAsync(
                ResolvePanelHandle(currentScreen),
                ResolvePanelTarget(currentScreen, targetScreen),
                false,
                token);

            await MovePanelAsync(
                ResolvePanelHandle(targetScreen),
                ResolvePanelTarget(targetScreen, targetScreen),
                true,
                token);

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
        
        _settingsPresenter.Dispose();
        _exitPresenter.Dispose();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeOneEvents();

        _settingsPresenter.Dispose();
        _exitPresenter.Dispose();
        
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

    private void EnsureLayoutCaptured()
    {
        if (_layoutCaptured)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        _menuShownPosition = ResolveShownPosition(_menuPanel.Panel, _layoutSettings.MenuShown);
        _settingsShownPosition = ResolveShownPosition(_settingsPanel.Panel, _layoutSettings.SettingsShown);
        _exitShownPosition = ResolveShownPosition(_exitPanel.Panel, _layoutSettings.ExitShown);
        _layoutCaptured = true;
    }

    private Vector2 ResolveShownPosition(RectTransform panel, Vector2 fallbackPosition)
    {
        return IsOffscreen(panel) ? fallbackPosition : panel.anchoredPosition;
    }

    private bool IsOffscreen(RectTransform panel)
    {
        var parent = panel.parent as RectTransform;
        if (parent == null)
        {
            return false;
        }

        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, panel);
        var parentRect = parent.rect;

        return bounds.max.x < parentRect.xMin
            || bounds.min.x > parentRect.xMax
            || bounds.max.y < parentRect.yMin
            || bounds.min.y > parentRect.yMax;
    }

    private Vector2 GetMenuTarget(MainMenuScreen screen)
    {
        return screen == MainMenuScreen.Menu
            ? _menuShownPosition
            : GetHiddenPosition(_menuPanel.Panel, _menuShownPosition, HideDirection.Up);
    }

    private Vector2 GetSettingsTarget(MainMenuScreen screen)
    {
        return screen == MainMenuScreen.Settings
            ? _settingsShownPosition
            : GetHiddenPosition(_settingsPanel.Panel, _settingsShownPosition, HideDirection.Left);
    }

    private Vector2 GetExitTarget(MainMenuScreen screen)
    {
        return screen == MainMenuScreen.ExitConfirm
            ? _exitShownPosition
            : GetHiddenPosition(_exitPanel.Panel, _exitShownPosition, HideDirection.Down);
    }

    private PanelHandle ResolvePanelHandle(MainMenuScreen screen)
    {
        return screen switch
        {
            MainMenuScreen.Menu => _menuPanel,
            MainMenuScreen.Settings => _settingsPanel,
            MainMenuScreen.ExitConfirm => _exitPanel,
            _ => _menuPanel
        };
    }

    private Vector2 ResolvePanelTarget(MainMenuScreen panelScreen, MainMenuScreen targetScreen)
    {
        return panelScreen switch
        {
            MainMenuScreen.Menu => GetMenuTarget(targetScreen),
            MainMenuScreen.Settings => GetSettingsTarget(targetScreen),
            MainMenuScreen.ExitConfirm => GetExitTarget(targetScreen),
            _ => GetMenuTarget(targetScreen)
        };
    }

    private Vector2 GetHiddenPosition(RectTransform panel, Vector2 shownPosition, HideDirection direction)
    {
        var parent = panel.parent as RectTransform;
        var padding = _layoutSettings.OffscreenPadding;

        if (parent == null)
        {
            var size = panel.rect.size;
            return direction switch
            {
                HideDirection.Up => shownPosition + Vector2.up * (size.y + padding),
                HideDirection.Down => shownPosition + Vector2.down * (size.y + padding),
                HideDirection.Left => shownPosition + Vector2.left * (size.x + padding),
                _ => shownPosition
            };
        }

        var parentRect = parent.rect;
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, panel);
        var deltaToShown = shownPosition - panel.anchoredPosition;
        bounds.center += (Vector3)deltaToShown;

        return direction switch
        {
            HideDirection.Up => shownPosition + new Vector2(0f, (parentRect.yMax + padding) - bounds.min.y),
            HideDirection.Down => shownPosition + new Vector2(0f, (parentRect.yMin - padding) - bounds.max.y),
            HideDirection.Left => shownPosition + new Vector2((parentRect.xMin - padding) - bounds.max.x, 0f),
            _ => shownPosition
        };
    }

    private enum HideDirection
    {
        Up,
        Left,
        Down
    }

    private readonly struct PanelHandle
    {
        public PanelHandle(
            RectTransform panel,
            Action show,
            Action hide,
            Action<bool> setInteractable,
            IReadOnlyList<RectTransform> elements)
        {
            Panel = panel;
            _show = show;
            _hide = hide;
            _setInteractable = setInteractable;
            Elements = elements;
        }

        public RectTransform Panel { get; }
        public IReadOnlyList<RectTransform> Elements { get; }
        private readonly Action _show;
        private readonly Action _hide;
        private readonly Action<bool> _setInteractable;

        public void Show()
        {
            _show?.Invoke();
        }

        public void Hide()
        {
            _hide?.Invoke();
        }

        public void SetInteractable(bool isInteractable)
        {
            _setInteractable?.Invoke(isInteractable);
        }
    }

    private void ApplyPanelImmediate(PanelHandle panel, Vector2 position, bool show)
    {
        MainMenuPanelAnimator.SetImmediate(panel.Panel, position);
        if (show)
        {
            panel.Show();
            panel.SetInteractable(true);
        }
        else
        {
            panel.SetInteractable(false);
            panel.Hide();
        }
    }

    private async UniTask MovePanelAsync(
        PanelHandle panel,
        Vector2 position,
        bool show,
        CancellationToken token)
    {
        var duration = _layoutSettings.TransitionDuration;
        var useUnscaledTime = _layoutSettings.UseUnscaledTime;
        var showOvershoot = _layoutSettings.ShowOvershoot;

        if (show)
        {
            panel.Show();
        }

        panel.SetInteractable(false);

        var startPosition = panel.Panel.anchoredPosition;
        var direction = position - startPosition;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.up;
        }

        await UniTask.WhenAll(
            MainMenuPanelAnimator.MoveAsync(
                panel.Panel,
                position,
                duration,
                useUnscaledTime,
                showOvershoot,
                token,
                show),
            MainMenuPanelContentAnimator.PlayAsync(
                panel.Panel,
                panel.Elements,
                show,
                direction,
                duration,
                useUnscaledTime,
                token));

        if (show)
        {
            panel.SetInteractable(true);
        }
        else
        {
            panel.Hide();
        }
    }
}
}