using System;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesDeleteConfirmView : GameProfilesDeleteConfirmViewBase
{
    [Header("Panel")]
    [SerializeField]
    private RectTransform _panel;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private bool _disableGameObjectOnHide = true;

    [Header("Buttons")]
    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private Button _cancelButton;

    private readonly AsyncEvent _confirmClicked = new();
    private readonly AsyncEvent _cancelClicked = new();
    private CancellationTokenSource _subscriptionsCts;
    private UniTask _subscriptionsTask;

    public override AsyncEvent ConfirmClicked => _confirmClicked;
    public override AsyncEvent CancelClicked => _cancelClicked;

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

    private UniTask RaiseConfirmClicked()
    {
        return _confirmClicked.InvokeAsync();
    }

    private UniTask RaiseCancelClicked()
    {
        return _cancelClicked.InvokeAsync();
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
