using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public static class MainMenuPanelAnimator
{
    public static void SetImmediate(RectTransform panel, Vector2 position)
    {
        panel.DOKill();
        panel.anchoredPosition = position;
    }

    public static async UniTask MoveAsync(
        RectTransform panel,
        Vector2 target,
        float duration,
        Ease ease,
        bool useUnscaledTime,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        panel.DOKill();

        if (duration <= 0f)
        {
            panel.anchoredPosition = target;
            return;
        }

        var tween = panel.DOAnchorPos(target, duration)
            .SetEase(ease)
            .SetUpdate(useUnscaledTime);

        using (token.Register(() => tween.Kill(false)))
        {
            await tween.AsyncWaitForCompletion();
        }
        token.ThrowIfCancellationRequested();
    }
}
}