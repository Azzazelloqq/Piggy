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

        var showMenu = screen == MainMenuScreen.Menu;
        var showSettings = screen == MainMenuScreen.Settings;
        var showExit = screen == MainMenuScreen.ExitConfirm;

        ApplyPanelImmediate(_view.MenuPanel, GetMenuTarget(screen), showMenu);
        ApplyPanelImmediate(_view.SettingsPanel, GetSettingsTarget(screen), showSettings);
        ApplyPanelImmediate(_view.ExitPanel, GetExitTarget(screen), showExit);

        _model.CurrentScreen = screen;
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
            var currentScreen = _model.CurrentScreen;

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

    private Vector2 GetMenuTarget(MainMenuScreen screen)
    {
        return screen == MainMenuScreen.Menu
            ? _model.MenuShownPosition
            : GetHiddenPosition(_view.MenuPanel.Panel, _model.MenuShownPosition, HideDirection.Up);
    }

    private Vector2 GetSettingsTarget(MainMenuScreen screen)
    {
        return screen == MainMenuScreen.Settings
            ? _model.SettingsShownPosition
            : GetHiddenPosition(_view.SettingsPanel.Panel, _model.SettingsShownPosition, HideDirection.Left);
    }

    private Vector2 GetExitTarget(MainMenuScreen screen)
    {
        return screen == MainMenuScreen.ExitConfirm
            ? _model.ExitShownPosition
            : GetHiddenPosition(_view.ExitPanel.Panel, _model.ExitShownPosition, HideDirection.Down);
    }

    private MainMenuScreenTransitionView.PanelHandle ResolvePanelHandle(MainMenuScreen screen)
    {
        return screen switch
        {
            MainMenuScreen.Menu => _view.MenuPanel,
            MainMenuScreen.Settings => _view.SettingsPanel,
            MainMenuScreen.ExitConfirm => _view.ExitPanel,
            _ => _view.MenuPanel
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
        var padding = _view.Layout.OffscreenPadding;

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
            HideDirection.Up => shownPosition + new Vector2(0f, parentRect.yMax + padding - bounds.min.y),
            HideDirection.Down => shownPosition + new Vector2(0f, parentRect.yMin - padding - bounds.max.y),
            HideDirection.Left => shownPosition + new Vector2(parentRect.xMin - padding - bounds.max.x, 0f),
            _ => shownPosition
        };
    }

    private enum HideDirection
    {
        Up,
        Left,
        Down
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
        var layout = _view.Layout;
        var duration = layout.TransitionDuration;
        var useUnscaledTime = layout.UseUnscaledTime;

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
                layout.PanelShowEase,
                layout.PanelShowOvershoot,
                layout.PanelShowSteps,
                layout.PanelHideEase,
                layout.PanelHideOvershoot,
                layout.PanelHideSteps,
                token,
                show),
            MainMenuPanelContentAnimator.PlayAsync(
                panel.Panel,
                panel.Elements,
                show,
                direction,
                duration,
                useUnscaledTime,
                layout.ContentShowEase,
                layout.ContentShowOvershoot,
                layout.ContentShowSteps,
                layout.ContentHideEase,
                layout.ContentHideOvershoot,
                layout.ContentHideSteps,
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