using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public static class MainMenuPanelContentAnimator
{
    private const float BaseOffset = 85f;
    private const float SideOffset = 36f;
    private const float StartScale = 0.9f;
    private const float EndScale = 0.88f;
    private const float MinDuration = 0.16f;
    private const float StaggerFactor = 0.12f;
    private const float MinStagger = 0.03f;
    private const float MaxStagger = 0.08f;
    private const float MaxJitter = 0.05f;
    private const float DurationJitter = 0.18f;

    public static async UniTask PlayAsync(
        RectTransform panel,
        IReadOnlyList<RectTransform> explicitElements,
        bool show,
        Vector2 direction,
        float duration,
        bool useUnscaledTime,
        Ease showEase,
        float showOvershoot,
        int showSteps,
        Ease hideEase,
        float hideOvershoot,
        int hideSteps,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (panel == null)
        {
            return;
        }

        var elements = CollectElements(panel, explicitElements, out var explicitOrder);
        if (elements.Count == 0)
        {
            return;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.up;
        }
        else
        {
            direction.Normalize();
        }

        var ordered = explicitOrder ? elements : OrderElements(elements, direction);
        var contentDuration = Mathf.Max(MinDuration, duration * 0.9f);
        var baseStagger = Mathf.Clamp(duration * StaggerFactor, MinStagger, MaxStagger);
        var maxTotalStagger = Mathf.Clamp(baseStagger * Mathf.Max(1, ordered.Count - 1), 0.12f, 0.35f);
        var offsetDirection = show ? -direction : direction;
        var perpendicular = new Vector2(-direction.y, direction.x);

        var layoutGroups = FreezeLayoutGroups(ordered, panel);
        var sequences = new List<Sequence>(ordered.Count);
        var tasks = new List<UniTask>(ordered.Count);

        try
        {
            for (var i = 0; i < ordered.Count; i++)
            {
                var rect = ordered[i];
                if (rect == null || !rect.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var state = GetOrCreateState(rect);
                CaptureStateIfNeeded(rect, state);

                var offset = offsetDirection * (BaseOffset * (0.7f + 0.5f * state.OffsetSeed))
                             + perpendicular * (SideOffset * (state.SideSeed * 2f - 1f));

                var orderT = ordered.Count > 1 ? i / (ordered.Count - 1f) : 0f;
                var delay = Mathf.SmoothStep(0f, maxTotalStagger, orderT)
                            + (state.DelaySeed - 0.5f) * MaxJitter;
                if (delay < 0f)
                {
                    delay = 0f;
                }

                rect.DOKill();
                state.CanvasGroup.DOKill();

                var elementDuration = contentDuration
                                      * Mathf.Lerp(1f - DurationJitter, 1f + DurationJitter, state.OffsetSeed);
                if (elementDuration < MinDuration)
                {
                    elementDuration = MinDuration;
                }

                var startPosition = show ? state.AnchoredPosition + offset : state.AnchoredPosition;
                var targetPosition = show ? state.AnchoredPosition : state.AnchoredPosition + offset;
                var startScale = show ? state.LocalScale * StartScale : state.LocalScale;
                var targetScale = show ? state.LocalScale : state.LocalScale * EndScale;
                var startAlpha = show ? 0f : state.Alpha;
                var targetAlpha = show ? state.Alpha : 0f;

                rect.anchoredPosition = startPosition;
                rect.localScale = startScale;
                state.CanvasGroup.alpha = startAlpha;

                var moveTween = rect.DOAnchorPos(targetPosition, elementDuration)
                    .SetUpdate(useUnscaledTime);
                var scaleTween = rect.DOScale(targetScale, elementDuration)
                    .SetUpdate(useUnscaledTime);
                var fadeTween = state.CanvasGroup.DOFade(targetAlpha, elementDuration)
                    .SetUpdate(useUnscaledTime);

                var ease = show ? showEase : hideEase;
                var overshoot = show ? showOvershoot : hideOvershoot;
                var steps = show ? showSteps : hideSteps;

                MainMenuAnimationEase.ApplyEase(moveTween, ease, overshoot, steps, elementDuration);
                MainMenuAnimationEase.ApplyEase(scaleTween, ease, overshoot, steps, elementDuration);
                MainMenuAnimationEase.ApplyEase(fadeTween, ease, overshoot, steps, elementDuration);

                var sequence = DOTween.Sequence()
                    .SetUpdate(useUnscaledTime)
                    .AppendInterval(delay)
                    .Append(moveTween)
                    .Join(scaleTween)
                    .Join(fadeTween);

                sequences.Add(sequence);
                tasks.Add(sequence.AsyncWaitForCompletion().AsUniTask());
            }

            if (tasks.Count == 0)
            {
                return;
            }

            await using (token.Register(() =>
                         {
                             foreach (var sequence in sequences)
                             {
                                 sequence.Kill(false);
                             }
                         }))
            {
                await UniTask.WhenAll(tasks);
            }
        }
        finally
        {
            RestoreLayoutGroups(layoutGroups);
        }

        token.ThrowIfCancellationRequested();
    }

    private static List<RectTransform> CollectElements(
        RectTransform panel,
        IReadOnlyList<RectTransform> explicitElements,
        out bool explicitOrder)
    {
        var results = new List<RectTransform>();
        var seen = new HashSet<RectTransform>();

        if (explicitElements != null && explicitElements.Count > 0)
        {
            explicitOrder = true;
            for (var i = 0; i < explicitElements.Count; i++)
            {
                var rect = explicitElements[i];
                if (rect == null || rect == panel)
                {
                    continue;
                }

                if (!rect.IsChildOf(panel))
                {
                    continue;
                }

                AddIfValid(panel, rect, results, seen);
            }

            if (results.Count > 0)
            {
                return results;
            }

            explicitOrder = false;
            results.Clear();
            seen.Clear();
        }

        explicitOrder = false;
        AddNamedElements(panel, results, seen);
        AddSelectables(panel, results, seen);
        AddLooseTexts(panel, results, seen);

        return results;
    }

    private static void AddNamedElements(
        RectTransform panel,
        List<RectTransform> results,
        HashSet<RectTransform> seen)
    {
        var rects = panel.GetComponentsInChildren<RectTransform>(true);
        for (var i = 0; i < rects.Length; i++)
        {
            var rect = rects[i];
            if (rect == null || rect == panel)
            {
                continue;
            }

            var name = rect.name;
            if (name.IndexOf("title", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("header", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("logo", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                AddIfValid(panel, rect, results, seen);
            }
        }
    }

    private static void AddSelectables(
        RectTransform panel,
        List<RectTransform> results,
        HashSet<RectTransform> seen)
    {
        var selectables = panel.GetComponentsInChildren<Selectable>(true);
        for (var i = 0; i < selectables.Length; i++)
        {
            var selectable = selectables[i];
            if (selectable == null)
            {
                continue;
            }

            AddIfValid(panel, selectable.transform as RectTransform, results, seen);
        }
    }

    private static void AddLooseTexts(
        RectTransform panel,
        List<RectTransform> results,
        HashSet<RectTransform> seen)
    {
        var texts = panel.GetComponentsInChildren<TMP_Text>(true);
        for (var i = 0; i < texts.Length; i++)
        {
            var text = texts[i];
            if (text == null)
            {
                continue;
            }

            if (text.GetComponentInParent<Selectable>() != null)
            {
                continue;
            }

            AddIfValid(panel, text.rectTransform, results, seen);
        }
    }

    private static void AddIfValid(
        RectTransform panel,
        RectTransform rect,
        List<RectTransform> results,
        HashSet<RectTransform> seen)
    {
        if (rect == null || rect == panel)
        {
            return;
        }

        if (seen.Contains(rect))
        {
            return;
        }

        if (HasAncestor(rect, seen, panel))
        {
            return;
        }

        seen.Add(rect);
        results.Add(rect);
    }

    private static bool HasAncestor(Transform target, HashSet<RectTransform> seen, RectTransform panel)
    {
        var current = target.parent;
        while (current != null && current != panel)
        {
            if (current is RectTransform rect && seen.Contains(rect))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static List<RectTransform> OrderElements(List<RectTransform> elements, Vector2 direction)
    {
        var ordered = new List<RectTransform>(elements);
        ordered.Sort((a, b) =>
        {
            var aKey = GetOrderKey(a, direction);
            var bKey = GetOrderKey(b, direction);
            return bKey.CompareTo(aKey);
        });

        return ordered;
    }

    private static float GetOrderKey(RectTransform rect, Vector2 direction)
    {
        var pos = rect.position;
        return pos.x * direction.x + pos.y * direction.y;
    }

    private static List<LayoutGroupState> FreezeLayoutGroups(
        List<RectTransform> elements,
        RectTransform panel)
    {
        var states = new List<LayoutGroupState>();
        var seen = new HashSet<LayoutGroup>();

        for (var i = 0; i < elements.Count; i++)
        {
            var rect = elements[i];
            if (rect == null)
            {
                continue;
            }

            var current = rect.parent;
            while (current != null && current != panel)
            {
                var group = current.GetComponent<LayoutGroup>();
                if (group != null && seen.Add(group))
                {
                    var groupRect = (RectTransform)group.transform;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(groupRect);
                    var wasEnabled = group.enabled;
                    if (wasEnabled)
                    {
                        group.enabled = false;
                    }

                    states.Add(new LayoutGroupState(group, wasEnabled));
                }

                current = current.parent;
            }
        }

        return states;
    }

    private static void RestoreLayoutGroups(List<LayoutGroupState> states)
    {
        for (var i = 0; i < states.Count; i++)
        {
            var state = states[i];
            if (state.Group == null)
            {
                continue;
            }

            if (state.WasEnabled)
            {
                state.Group.enabled = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)state.Group.transform);
            }
        }
    }

    private static MainMenuPanelElementState GetOrCreateState(RectTransform rect)
    {
        var state = rect.GetComponent<MainMenuPanelElementState>();
        if (state == null)
        {
            state = rect.gameObject.AddComponent<MainMenuPanelElementState>();
        }

        if (state.CanvasGroup == null)
        {
            state.CanvasGroup = rect.GetComponent<CanvasGroup>();
            if (state.CanvasGroup == null)
            {
                state.CanvasGroup = rect.gameObject.AddComponent<CanvasGroup>();
            }
        }

        return state;
    }

    private static void CaptureStateIfNeeded(RectTransform rect, MainMenuPanelElementState state)
    {
        if (state.Initialized)
        {
            return;
        }

        state.AnchoredPosition = rect.anchoredPosition;
        state.LocalScale = rect.localScale;
        state.Alpha = state.CanvasGroup.alpha;

        var seed = rect.GetInstanceID();
        state.OffsetSeed = Hash01(seed * 17 + 13);
        state.SideSeed = Hash01(seed * 31 + 7);
        state.DelaySeed = Hash01(seed * 53 + 3);
        state.Initialized = true;
    }

    private static float Hash01(int seed)
    {
        unchecked
        {
            var hash = seed;
            hash ^= hash << 13;
            hash ^= hash >> 17;
            hash ^= hash << 5;
            return (hash & 0x7fffffff) / (float)int.MaxValue;
        }
    }
}

internal sealed class MainMenuPanelElementState : MonoBehaviour
{
    public bool Initialized;
    public Vector2 AnchoredPosition;
    public Vector3 LocalScale;
    public float Alpha;
    public float OffsetSeed;
    public float SideSeed;
    public float DelaySeed;
    public CanvasGroup CanvasGroup;
}

internal readonly struct LayoutGroupState
{
    public LayoutGroupState(LayoutGroup group, bool wasEnabled)
    {
        Group = group;
        WasEnabled = wasEnabled;
    }

    public LayoutGroup Group { get; }
    public bool WasEnabled { get; }
}
}