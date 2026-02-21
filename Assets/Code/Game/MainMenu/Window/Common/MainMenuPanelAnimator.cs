using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public static class MainMenuPanelAnimator
{
    private const Ease HideEase = Ease.InCubic;

    public static void SetImmediate(RectTransform panel, Vector2 position)
    {
        panel.DOKill();
        panel.anchoredPosition = position;
    }

    public static async UniTask MoveAsync(
        RectTransform panel,
        Vector2 target,
        float duration,
        bool useUnscaledTime,
        float showOvershoot,
        CancellationToken token,
        bool show)
    {
        token.ThrowIfCancellationRequested();
        panel.DOKill();

        if ((panel.anchoredPosition - target).sqrMagnitude <= 0.01f)
        {
            panel.anchoredPosition = target;
            return;
        }

        if (duration <= 0f)
        {
            panel.anchoredPosition = target;
            return;
        }

        var tween = show
            ? CreateShowTween(panel, target, duration, useUnscaledTime, showOvershoot)
            : CreateHideTween(panel, target, duration, useUnscaledTime);

        await using (token.Register(() => tween.Kill(false)))
        {
            await tween.AsyncWaitForCompletion();
        }

        token.ThrowIfCancellationRequested();
    }

    private static Tween CreateShowTween(
        RectTransform panel,
        Vector2 target,
        float duration,
        bool useUnscaledTime,
        float showOvershoot)
    {
        return panel.DOAnchorPos(target, duration)
            .SetEase(Ease.OutBack, showOvershoot)
            .SetUpdate(useUnscaledTime);
    }

    private static Tween CreateHideTween(
        RectTransform panel,
        Vector2 target,
        float duration,
        bool useUnscaledTime)
    {
        return panel.DOAnchorPos(target, duration)
            .SetEase(HideEase)
            .SetUpdate(useUnscaledTime);
    }
}
}