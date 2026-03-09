using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesView : GameProfilesViewBase
{
    [Header("Panel")]
    [SerializeField]
    private RectTransform _panel;

    [Header("Content Animation")]
    [SerializeField]
    private RectTransform[] _animatedElements;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private bool _disableGameObjectOnHide = true;

    [Header("Navigation")]
    [SerializeField]
    private Button _backButton;

    [Header("Popups")]
    [SerializeField]
    private GameProfilesDeleteConfirmViewBase _deleteConfirmView;

    [Header("Slots")]
    [SerializeField]
    private ScrollRect _scrollRect;

    [SerializeField]
    private RectTransform _contentRoot;

    [SerializeField]
    private GameProfilesSlotViewBase _slotPrefab;

    private readonly List<GameProfilesSlotViewBase> _slotViews = new();
    private readonly AsyncEvent _backClicked = new();
    private CancellationTokenSource _subscriptionsCts;
    private UniTask _subscriptionsTask;

    public override AsyncEvent BackClicked => _backClicked;
    public override RectTransform Panel => _panel;
    public override IReadOnlyList<RectTransform> AnimatedElements => _animatedElements ?? Array.Empty<RectTransform>();
    public override GameProfilesDeleteConfirmViewBase DeleteConfirmView => _deleteConfirmView;

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

    public override IReadOnlyList<GameProfilesSlotViewBase> EnsureSlotViews(int count)
    {
        var root = ResolveContentRoot();
        EnsureSlotCount(count, root);
        _scrollRect.verticalNormalizedPosition = 1f;
        return _slotViews;
    }

    protected override void OnInitialize()
    {
        SubscribeOnEvents(default);
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeOnEvents(token);
        return default;
    }

    protected override void OnDispose()
    {
        StopSubscriptionsImmediate();
    }

    protected override async ValueTask OnDisposeAsync(CancellationToken token)
    {
        await StopSubscriptionsAsync();
    }

    private RectTransform ResolveContentRoot()
    {
        if (_contentRoot == null)
        {
            _contentRoot = _scrollRect.content;
        }
        return _contentRoot;
    }

    private void EnsureSlotCount(int count, RectTransform root)
    {
        while (_slotViews.Count < count)
        {
            var instance = Instantiate(_slotPrefab, root, false);
            _slotViews.Add(instance);
        }

        while (_slotViews.Count > count)
        {
            var lastIndex = _slotViews.Count - 1;
            Destroy(_slotViews[lastIndex].gameObject);
            _slotViews.RemoveAt(lastIndex);
        }
    }

    private UniTask RaiseBackClicked()
    {
        return _backClicked.InvokeAsync();
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
        await WaitForClicksAsync(_backButton, RaiseBackClicked, token);
    }

    private static async UniTask WaitForClicksAsync(
        Button button,
        Func<UniTask> onClick,
        CancellationToken token)
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
}
}
