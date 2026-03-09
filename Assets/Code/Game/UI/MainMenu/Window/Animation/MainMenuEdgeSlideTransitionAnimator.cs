using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuEdgeSlideTransitionAnimator : IMainMenuScreenTransitionAnimator
{
    public MainMenuScreenTransitionDirection GetDefaultEnterDirection(MainMenuScreen screen)
    {
        return screen switch
        {
            MainMenuScreen.Settings => MainMenuScreenTransitionDirection.Left,
            MainMenuScreen.ExitConfirm => MainMenuScreenTransitionDirection.Down,
            MainMenuScreen.Saves => MainMenuScreenTransitionDirection.Up,
            MainMenuScreen.CharacterCreation => MainMenuScreenTransitionDirection.Right,
            _ => MainMenuScreenTransitionDirection.Up
        };
    }

    public Vector2 GetHiddenPosition(
        RectTransform panel,
        Vector2 shownPosition,
        MainMenuScreenTransitionDirection direction,
        MainMenuViewBase.LayoutData layout)
    {
        var parent = panel.parent as RectTransform;
        var padding = layout.OffscreenPadding;

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

    public void ApplyPanelImmediate(
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

    public void PreparePanelForShow(
        MainMenuScreenTransitionView.PanelHandle panel,
        Vector2 hiddenPosition)
    {
        MainMenuPanelAnimator.SetImmediate(panel.Panel, hiddenPosition);
    }

    public async UniTask MovePanelAsync(
        MainMenuScreenTransitionView.PanelHandle panel,
        Vector2 position,
        bool show,
        MainMenuViewBase.LayoutData layout,
        CancellationToken token)
    {
        var duration = layout.TransitionDuration;
        var useUnscaledTime = layout.UseUnscaledTime;
        var showOvershoot = layout.ShowOvershoot;

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
