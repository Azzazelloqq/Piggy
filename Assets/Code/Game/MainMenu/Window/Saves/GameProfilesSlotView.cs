using System;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesSlotView : GameProfilesSlotViewBase
{
    [SerializeField]
    private Button _button;

    [SerializeField]
    private TMP_Text _slotLabel;

    [SerializeField]
    private GameObject _emptyState;

    [SerializeField]
    private GameObject _filledState;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    private readonly AsyncEvent _clicked = new();
    private CancellationTokenSource _subscriptionsCts;
    private UniTask _subscriptionsTask;

    public override AsyncEvent Clicked => _clicked;

    public override void SetData(GameProfilesSlotData data)
    {
        _slotLabel.text = (data.Index + 1).ToString();
        _emptyState.SetActive(!data.HasSave);
        _filledState.SetActive(data.HasSave);
    }

    public override void SetInteractable(bool isInteractable)
    {
        _button.interactable = isInteractable;
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

    private UniTask RaiseClicked()
    {
        return _clicked.InvokeAsync();
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
        await WaitForClicksAsync(_button, RaiseClicked, token);
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
