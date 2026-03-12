using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuPaperTransitionAnimator : IMainMenuScreenTransitionAnimator
{
    private const int Fps = 12; // Stop-motion effect FPS

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
            MovePaperAsync(panel.Panel, startPosition, position, duration, useUnscaledTime, show, token),
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

    private async UniTask MovePaperAsync(
        RectTransform panel,
        Vector2 startPos,
        Vector2 endPos,
        float duration,
        bool useUnscaledTime,
        bool show,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        panel.DOKill();

        if (duration <= 0f || (startPos - endPos).sqrMagnitude <= 0.01f)
        {
            panel.anchoredPosition = endPos;
            return;
        }

        float elapsed = 0f;
        float frameDuration = 1f / Fps;
        float nextFrameTime = 0f;

        // Для эффекта бумаги добавим небольшую случайную ротацию и скейл
        Vector3 initialScale = panel.localScale;
        
        while (elapsed < duration)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;

            if (elapsed >= nextFrameTime)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Используем OutBack для появления (бумага слегка перелетает и возвращается)
                // И InCubic для исчезновения
                float easeT = show 
                    ? DOVirtual.EasedValue(0, 1, t, Ease.OutBack, 1.2f) 
                    : DOVirtual.EasedValue(0, 1, t, Ease.InCubic);
                
                panel.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, easeT);
                
                // Эффект "небрежности" при движении бумаги
                // Чем ближе к концу, тем меньше искажений
                float noiseIntensity = 1f - t;
                
                if (show && t < 1f)
                {
                    float randomRot = UnityEngine.Random.Range(-2f, 2f) * noiseIntensity;
                    panel.localRotation = Quaternion.Euler(0, 0, randomRot);
                    
                    float randomScale = 1f + UnityEngine.Random.Range(-0.05f, 0.05f) * noiseIntensity;
                    panel.localScale = initialScale * randomScale;
                }

                nextFrameTime += frameDuration;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        panel.anchoredPosition = endPos;
        panel.localRotation = Quaternion.identity;
        panel.localScale = initialScale;
    }
}
}