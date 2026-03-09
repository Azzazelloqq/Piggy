using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuScreenTransitionPresenter
{
    private readonly MainMenuScreenTransitionView _view;
    private readonly MainMenuScreenTransitionModel _model;
    private readonly IMainMenuScreenTransitionAnimator _animator;

    public MainMenuScreenTransitionPresenter(
        MainMenuScreenTransitionView view,
        MainMenuScreenTransitionModel model,
        IMainMenuScreenTransitionAnimator animator)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _animator = animator ?? throw new ArgumentNullException(nameof(animator));
    }

    public void ApplyScreenLayoutImmediate(MainMenuScreen screen)
    {
        EnsureLayoutCaptured();

        _animator.ApplyPanelImmediate(
            _view.MenuPanel,
            GetTargetPosition(MainMenuScreen.Menu, screen),
            screen == MainMenuScreen.Menu);
        _animator.ApplyPanelImmediate(
            _view.SettingsPanel,
            GetTargetPosition(MainMenuScreen.Settings, screen),
            screen == MainMenuScreen.Settings);
        _animator.ApplyPanelImmediate(
            _view.ExitPanel,
            GetTargetPosition(MainMenuScreen.ExitConfirm, screen),
            screen == MainMenuScreen.ExitConfirm);
        _animator.ApplyPanelImmediate(
            _view.SavesPanel,
            GetTargetPosition(MainMenuScreen.Saves, screen),
            screen == MainMenuScreen.Saves);
        _animator.ApplyPanelImmediate(
            _view.CharacterCreationPanel,
            GetTargetPosition(MainMenuScreen.CharacterCreation, screen),
            screen == MainMenuScreen.CharacterCreation);

        _model.CurrentScreen = screen;
        ResetStack(screen);
    }

    public async UniTask<bool> TryTransitionToScreenAsync(MainMenuScreen targetScreen, CancellationToken token)
    {
        EnsureLayoutCaptured();

        if (_model.IsTransitioning || _model.CurrentScreen == targetScreen)
        {
            return false;
        }

        _model.IsTransitioning = true;
        try
        {
            EnsureScreenStackInitialized(_model.CurrentScreen);

            var currentScreen = _model.CurrentScreen;
            var isBack = IsBackTransition(targetScreen);

            MainMenuScreenTransitionDirection targetEnterDirection;
            MainMenuScreenTransitionDirection currentHideDirection;

            if (isBack)
            {
                var currentEntry = _model.ScreenStack[_model.ScreenStack.Count - 1];
                currentHideDirection = currentEntry.EnterDirection;
                _model.ScreenStack.RemoveAt(_model.ScreenStack.Count - 1);
                targetEnterDirection = Opposite(currentHideDirection);
            }
            else
            {
                targetEnterDirection = _animator.GetDefaultEnterDirection(targetScreen);
                _model.ScreenStack.Add(new MainMenuScreenTransitionEntry(targetScreen, targetEnterDirection));
                currentHideDirection = Opposite(targetEnterDirection);
            }

            var currentPanel = ResolvePanelHandle(currentScreen);
            var targetPanel = ResolvePanelHandle(targetScreen);

            var currentHiddenPosition = _animator.GetHiddenPosition(
                currentPanel.Panel,
                GetShownPosition(currentScreen),
                currentHideDirection,
                _view.Layout);
            var targetShownPosition = GetShownPosition(targetScreen);
            var targetHiddenPosition = _animator.GetHiddenPosition(
                targetPanel.Panel,
                targetShownPosition,
                targetEnterDirection,
                _view.Layout);

            _animator.PreparePanelForShow(targetPanel, targetHiddenPosition);

            await _animator.MovePanelAsync(currentPanel, currentHiddenPosition, false, _view.Layout, token);
            await _animator.MovePanelAsync(targetPanel, targetShownPosition, true, _view.Layout, token);

            _model.CurrentScreen = targetScreen;
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            _model.IsTransitioning = false;
        }
    }

    private void EnsureLayoutCaptured()
    {
        if (_model.LayoutCaptured)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        _model.MenuShownPosition = ResolveShownPosition(_view.MenuPanel.Panel, _view.Layout.MenuShown);
        _model.SettingsShownPosition = ResolveShownPosition(_view.SettingsPanel.Panel, _view.Layout.SettingsShown);
        _model.ExitShownPosition = ResolveShownPosition(_view.ExitPanel.Panel, _view.Layout.ExitShown);
        _model.SavesShownPosition = ResolveShownPosition(_view.SavesPanel.Panel, _view.Layout.SavesShown);
        _model.CharacterCreationShownPosition = ResolveShownPosition(
            _view.CharacterCreationPanel.Panel,
            _view.Layout.CharacterCreationShown);
        _model.LayoutCaptured = true;
    }

    private Vector2 ResolveShownPosition(RectTransform panel, Vector2 fallbackPosition)
    {
        return IsOffscreen(panel) ? fallbackPosition : panel.anchoredPosition;
    }

    private static bool IsOffscreen(RectTransform panel)
    {
        if (!panel.gameObject.activeInHierarchy)
        {
            return true;
        }

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

    private Vector2 GetTargetPosition(MainMenuScreen panelScreen, MainMenuScreen activeScreen)
    {
        if (panelScreen == activeScreen)
        {
            return GetShownPosition(panelScreen);
        }

        var direction = ResolveHiddenDirection(panelScreen, activeScreen);
        return _animator.GetHiddenPosition(
            ResolvePanelHandle(panelScreen).Panel,
            GetShownPosition(panelScreen),
            direction,
            _view.Layout);
    }

    private Vector2 GetShownPosition(MainMenuScreen screen)
    {
        return screen switch
        {
            MainMenuScreen.Menu => _model.MenuShownPosition,
            MainMenuScreen.Settings => _model.SettingsShownPosition,
            MainMenuScreen.ExitConfirm => _model.ExitShownPosition,
            MainMenuScreen.Saves => _model.SavesShownPosition,
            MainMenuScreen.CharacterCreation => _model.CharacterCreationShownPosition,
            _ => _model.MenuShownPosition
        };
    }

    private MainMenuScreenTransitionDirection ResolveHiddenDirection(
        MainMenuScreen panelScreen,
        MainMenuScreen activeScreen)
    {
        if (panelScreen == MainMenuScreen.Menu)
        {
            var activeEnter = _animator.GetDefaultEnterDirection(activeScreen);
            return Opposite(activeEnter);
        }

        return _animator.GetDefaultEnterDirection(panelScreen);
    }

    private void ResetStack(MainMenuScreen screen)
    {
        _model.ScreenStack.Clear();
        var enterDirection = _animator.GetDefaultEnterDirection(screen);
        _model.ScreenStack.Add(new MainMenuScreenTransitionEntry(screen, enterDirection));
    }

    private void EnsureScreenStackInitialized(MainMenuScreen screen)
    {
        if (_model.ScreenStack.Count > 0)
        {
            return;
        }

        var enterDirection = _animator.GetDefaultEnterDirection(screen);
        _model.ScreenStack.Add(new MainMenuScreenTransitionEntry(screen, enterDirection));
    }

    private bool IsBackTransition(MainMenuScreen targetScreen)
    {
        if (_model.ScreenStack.Count < 2)
        {
            return false;
        }

        var previous = _model.ScreenStack[_model.ScreenStack.Count - 2];
        return previous.Screen == targetScreen;
    }

    private static MainMenuScreenTransitionDirection Opposite(MainMenuScreenTransitionDirection direction)
    {
        return direction switch
        {
            MainMenuScreenTransitionDirection.Up => MainMenuScreenTransitionDirection.Down,
            MainMenuScreenTransitionDirection.Down => MainMenuScreenTransitionDirection.Up,
            MainMenuScreenTransitionDirection.Left => MainMenuScreenTransitionDirection.Right,
            MainMenuScreenTransitionDirection.Right => MainMenuScreenTransitionDirection.Left,
            _ => MainMenuScreenTransitionDirection.Up
        };
    }

    private MainMenuScreenTransitionView.PanelHandle ResolvePanelHandle(MainMenuScreen screen)
    {
        return screen switch
        {
            MainMenuScreen.Menu => _view.MenuPanel,
            MainMenuScreen.Settings => _view.SettingsPanel,
            MainMenuScreen.ExitConfirm => _view.ExitPanel,
            MainMenuScreen.Saves => _view.SavesPanel,
            MainMenuScreen.CharacterCreation => _view.CharacterCreationPanel,
            _ => _view.MenuPanel
        };
    }
}
}