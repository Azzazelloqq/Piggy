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
        bool useUnscaledTime,
        Ease showEase,
        float showOvershoot,
        int showSteps,
        Ease hideEase,
        float hideOvershoot,
        int hideSteps,
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

        var tween = panel.DOAnchorPos(target, duration)
            .SetUpdate(useUnscaledTime);

        var ease = show ? showEase : hideEase;
        var overshoot = show ? showOvershoot : hideOvershoot;
        var steps = show ? showSteps : hideSteps;
        MainMenuAnimationEase.ApplyEase(tween, ease, overshoot, steps, duration);

        await using (token.Register(() => tween.Kill(false)))
        {
            await tween.AsyncWaitForCompletion();
        }

        token.ThrowIfCancellationRequested();
    }

}

internal static class MainMenuAnimationEase
{
    public static void ApplyEase(Tween tween, Ease ease, float overshoot, int steps, float duration)
    {
        if (tween == null)
        {
            return;
        }

        var stopMotionFps = ResolveStopMotionFps(steps, duration);
        if (stopMotionFps > 1)
        {
            var eased = EaseFactory.StopMotion(stopMotionFps, (time, tweenDuration, _, _) =>
            {
                var t = tweenDuration <= 0f ? 1f : Mathf.Clamp01(time / tweenDuration);
                return DOVirtual.EasedValue(0f, 1f, t, ease, overshoot);
            });
            tween.SetEase(eased);
            return;
        }

        tween.SetEase(ease, overshoot);
    }

    private static int ResolveStopMotionFps(int steps, float duration)
    {
        if (steps <= 1 || duration <= 0f)
        {
            return 0;
        }

        var fps = Mathf.CeilToInt(steps / duration);
        return Mathf.Max(1, fps);
    }
}
}