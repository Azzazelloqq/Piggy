using System;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using LocalizedDomain.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public sealed class CharacterStatRowView : CharacterStatRowViewBase
{
    [SerializeField]
    private Button _increaseButton;

    [SerializeField]
    private Button _decreaseButton;

    [SerializeField]
    private TMP_Text _valueText;

    [SerializeField]
    private LocalizedTMPText _labelText;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    private readonly AsyncEvent _incrementClicked = new();
    private readonly AsyncEvent _decrementClicked = new();
    private CancellationTokenSource _subscriptionsCts;
    private UniTask _subscriptionsTask;

    public override AsyncEvent IncrementClicked => _incrementClicked;
    public override AsyncEvent DecrementClicked => _decrementClicked;

    public void Bind(
        LocalizedTMPText labelText,
        TMP_Text valueText,
        Button decreaseButton,
        Button increaseButton,
        CanvasGroup canvasGroup)
    {
        _labelText = labelText;
        _valueText = valueText;
        _decreaseButton = decreaseButton;
        _increaseButton = increaseButton;
        _canvasGroup = canvasGroup;
    }

    public override void SetData(CharacterStatRowData data)
    {
        _labelText.Key = data.LocalizationKey;
        _labelText.Fallback = data.FallbackLabel;
        _valueText.text = data.Value.ToString();
        _increaseButton.interactable = data.CanIncrease;
        _decreaseButton.interactable = data.CanDecrease;
    }

    public override void SetInteractable(bool isInteractable)
    {
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.blocksRaycasts = isInteractable;
        _increaseButton.interactable = isInteractable;
        _decreaseButton.interactable = isInteractable;
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

    private UniTask RaiseIncrementClicked()
    {
        return _incrementClicked.InvokeAsync();
    }

    private UniTask RaiseDecrementClicked()
    {
        return _decrementClicked.InvokeAsync();
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
            WaitForClicksAsync(_increaseButton, RaiseIncrementClicked, token),
            WaitForClicksAsync(_decreaseButton, RaiseDecrementClicked, token));
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
                    await onClick();
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
