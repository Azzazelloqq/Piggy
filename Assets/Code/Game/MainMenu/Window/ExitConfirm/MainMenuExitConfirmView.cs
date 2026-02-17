using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuExitConfirmView : MainMenuExitConfirmViewBase
{
    [Header("Panel")]
    [SerializeField]
    private RectTransform _panel;

    [Header("Content Animation")]
    [SerializeField]
    private RectTransform[] _animatedElements;

    private RectTransform _resolvedPanel;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private bool _disableGameObjectOnHide = true;

    [Header("Buttons")]
    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private Button _cancelButton;

    private CancellationTokenSource _subscriptionsCts;
    private UniTask _subscriptionsTask;

    public override RectTransform Panel => _resolvedPanel ??= ResolvePanel();
    public override IReadOnlyList<RectTransform> AnimatedElements => _animatedElements ?? Array.Empty<RectTransform>();

    public override void SetVisible(bool isVisible)
    {
        _canvasGroup.alpha = isVisible ? 1f : 0f;
        _canvasGroup.interactable = isVisible;
        _canvasGroup.blocksRaycasts = isVisible;

        if (_disableGameObjectOnHide)
        {
            gameObject.SetActive(isVisible);
        }
    }

    public override void SetInteractable(bool isInteractable)
    {
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.blocksRaycasts = isInteractable;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        SubscribeOnEvents(default);
    }

    protected override async ValueTask OnInitializeAsync(CancellationToken token)
    {
        await base.OnInitializeAsync(token);
        SubscribeOnEvents(token);
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        StopSubscriptionsImmediate();
    }

    protected override async ValueTask OnDisposeAsync(CancellationToken token)
    {
        await base.OnDisposeAsync(token);
        await StopSubscriptionsAsync();
    }

    private void SubscribeOnEvents(CancellationToken token)
    {
        StopSubscriptionsImmediate();

        _subscriptionsCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            this.GetCancellationTokenOnDestroy());
        _subscriptionsTask = RunButtonSubscriptionsAsync(_subscriptionsCts.Token);
    }

    private void StopSubscriptionsImmediate()
    {
        if (_subscriptionsCts == null)
        {
            return;
        }

        _subscriptionsCts.Cancel();
        _subscriptionsCts.Dispose();
        _subscriptionsCts = null;
        _subscriptionsTask = default;
    }

    private async UniTask StopSubscriptionsAsync()
    {
        if (_subscriptionsCts == null)
        {
            return;
        }

        _subscriptionsCts.Cancel();
        _subscriptionsCts.Dispose();
        _subscriptionsCts = null;

        await _subscriptionsTask;
        _subscriptionsTask = default;
    }

    private async UniTask RunButtonSubscriptionsAsync(CancellationToken token)
    {
        await UniTask.WhenAll(
            WaitForClicksAsync(_confirmButton, RaiseConfirmClicked, token),
            WaitForClicksAsync(_cancelButton, RaiseCancelClicked, token));
    }

    private static async UniTask WaitForClicksAsync(Button button, Func<UniTask> onClick, CancellationToken token)
    {
        try
        {
            await foreach (var _ in button.OnClickAsAsyncEnumerable(token))
            {
                try
                {
                    if (onClick != null)
                    {
                        await onClick();
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private RectTransform ResolvePanel()
    {
        var root = (RectTransform)transform;
        if (_panel == null || _panel == root)
        {
            return root;
        }

        return IsOffscreen(root) ? root : _panel;
    }

    private static bool IsOffscreen(RectTransform rect)
    {
        var parent = rect.parent as RectTransform;
        if (parent == null)
        {
            return false;
        }

        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, rect);
        var parentRect = parent.rect;

        return bounds.max.x < parentRect.xMin
            || bounds.min.x > parentRect.xMax
            || bounds.max.y < parentRect.yMin
            || bounds.min.y > parentRect.yMax;
    }
}
}