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

    public MainMenuScreenTransitionPresenter(
        MainMenuScreenTransitionView view,
        MainMenuScreenTransitionModel model)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public void ApplyScreenLayoutImmediate(MainMenuScreen screen)
    {
        EnsureLayoutCaptured();

        ApplyPanelImmediate(
            _view.MenuPanel,
            GetTargetPosition(MainMenuScreen.Menu, screen),
            screen == MainMenuScreen.Menu);
        ApplyPanelImmediate(
            _view.SettingsPanel,
            GetTargetPosition(MainMenuScreen.Settings, screen),
            screen == MainMenuScreen.Settings);
        ApplyPanelImmediate(
            _view.ExitPanel,
            GetTargetPosition(MainMenuScreen.ExitConfirm, screen),
            screen == MainMenuScreen.ExitConfirm);
        ApplyPanelImmediate(
            _view.SavesPanel,
            GetTargetPosition(MainMenuScreen.Saves, screen),
            screen == MainMenuScreen.Saves);

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
                targetEnterDirection = ResolveDefaultEnterDirection(targetScreen);
                _model.ScreenStack.Add(new MainMenuScreenTransitionEntry(targetScreen, targetEnterDirection));
                currentHideDirection = Opposite(targetEnterDirection);
            }

            var currentPanel = ResolvePanelHandle(currentScreen);
            var targetPanel = ResolvePanelHandle(targetScreen);

            var currentHiddenPosition = GetHiddenPosition(
                currentPanel.Panel,
                GetShownPosition(currentScreen),
                currentHideDirection);
            var targetShownPosition = GetShownPosition(targetScreen);
            var targetHiddenPosition = GetHiddenPosition(
                targetPanel.Panel,
                targetShownPosition,
                targetEnterDirection);

            MainMenuPanelAnimator.SetImmediate(targetPanel.Panel, targetHiddenPosition);

            await MovePanelAsync(currentPanel, currentHiddenPosition, false, token);
            await MovePanelAsync(targetPanel, targetShownPosition, true, token);

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
        return GetHiddenPosition(ResolvePanelHandle(panelScreen).Panel, GetShownPosition(panelScreen), direction);
    }

    private Vector2 GetShownPosition(MainMenuScreen screen)
    {
        return screen switch
        {
            MainMenuScreen.Menu => _model.MenuShownPosition,
            MainMenuScreen.Settings => _model.SettingsShownPosition,
            MainMenuScreen.ExitConfirm => _model.ExitShownPosition,
            MainMenuScreen.Saves => _model.SavesShownPosition,
            _ => _model.MenuShownPosition
        };
    }

    private MainMenuScreenTransitionDirection ResolveHiddenDirection(
        MainMenuScreen panelScreen,
        MainMenuScreen activeScreen)
    {
        if (panelScreen == MainMenuScreen.Menu)
        {
            var activeEnter = ResolveDefaultEnterDirection(activeScreen);
            return Opposite(activeEnter);
        }

        return ResolveDefaultEnterDirection(panelScreen);
    }

    private MainMenuScreenTransitionDirection ResolveDefaultEnterDirection(MainMenuScreen screen)
    {
        return screen switch
        {
            MainMenuScreen.Settings => MainMenuScreenTransitionDirection.Left,
            MainMenuScreen.ExitConfirm => MainMenuScreenTransitionDirection.Down,
            MainMenuScreen.Saves => MainMenuScreenTransitionDirection.Up,
            _ => MainMenuScreenTransitionDirection.Up
        };
    }

    private void ResetStack(MainMenuScreen screen)
    {
        _model.ScreenStack.Clear();
        var enterDirection = ResolveDefaultEnterDirection(screen);
        _model.ScreenStack.Add(new MainMenuScreenTransitionEntry(screen, enterDirection));
    }

    private void EnsureScreenStackInitialized(MainMenuScreen screen)
    {
        if (_model.ScreenStack.Count > 0)
        {
            return;
        }

        var enterDirection = ResolveDefaultEnterDirection(screen);
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
            _ => _view.MenuPanel
        };
    }

    private Vector2 GetHiddenPosition(
        RectTransform panel,
        Vector2 shownPosition,
        MainMenuScreenTransitionDirection direction)
    {
        var parent = panel.parent as RectTransform;
        var padding = _view.Layout.OffscreenPadding;

        if (parent == null)
        {
            var size = panel.rect.size;
            return direction switch
            {
                MainMenuScreenTransitionDirection.Up => shownPosition + Vector2.up * (size.y + padding),
                MainMenuScreenTransitionDirection.Down => shownPosition + Vector2.down * (size.y + padding),
                MainMenuScreenTransitionDirection.Left => shownPosition + Vector2.left * (size.x + padding),
                MainMenuScreenTransitionDirection.Right => shownPosition + Vector2.right * (size.x + padding),
                _ => shownPosition
            };
        }

        var parentRect = parent.rect;
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, panel);
        var deltaToShown = shownPosition - panel.anchoredPosition;
        bounds.center += (Vector3)deltaToShown;

        return direction switch
        {
            MainMenuScreenTransitionDirection.Up => shownPosition + new Vector2(0f, parentRect.yMax + padding - bounds.min.y),
            MainMenuScreenTransitionDirection.Down => shownPosition + new Vector2(0f, parentRect.yMin - padding - bounds.max.y),
            MainMenuScreenTransitionDirection.Left => shownPosition + new Vector2(parentRect.xMin - padding - bounds.max.x, 0f),
            MainMenuScreenTransitionDirection.Right => shownPosition + new Vector2(parentRect.xMax + padding - bounds.min.x, 0f),
            _ => shownPosition
        };
    }

    private void ApplyPanelImmediate(
        MainMenuScreenTransitionView.PanelHandle panel,
        Vector2 position,
        bool show)
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
        MainMenuScreenTransitionView.PanelHandle panel,
        Vector2 position,
        bool show,
        CancellationToken token)
    {
        var duration = _view.Layout.TransitionDuration;
        var useUnscaledTime = _view.Layout.UseUnscaledTime;
        var showOvershoot = _view.Layout.ShowOvershoot;

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