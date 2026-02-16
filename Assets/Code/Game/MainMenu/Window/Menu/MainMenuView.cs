using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuView : MainMenuViewBase
{
    [Header("Layout")]
    [SerializeField]
    private LayoutData _layout = new LayoutData();

    [Header("Panel")]
    [SerializeField]
    private RectTransform _panel;

    [Header("Content Animation")]
    [SerializeField]
    private RectTransform[] _animatedElements;

    [Header("Child Views")]
    [SerializeField]
    private MainMenuSettingsViewBase _settingsView;

    [SerializeField]
    private MainMenuExitConfirmViewBase _exitConfirmView;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private bool _disableGameObjectOnHide = true;

    [Header("Buttons")]
    [SerializeField]
    private Button _playButton;

    [SerializeField]
    private Button _settingsButton;

    [SerializeField]
    private Button _exitButton;

    private CancellationTokenSource _subscriptionsCts;
    private UniTask _subscriptionsTask;

    public override RectTransform Panel => _panel;
    public override LayoutData Layout => _layout;
    public override IReadOnlyList<RectTransform> AnimatedElements => _animatedElements ?? Array.Empty<RectTransform>();

    internal override MainMenuSettingsViewBase SettingsView => _settingsView;
    internal override MainMenuExitConfirmViewBase ExitConfirmView => _exitConfirmView;

    public override void SetVisible(bool isVisible)
    {
        _canvasGroup.alpha = isVisible ? 1f : 0f;
        _canvasGroup.interactable = isVisible;
        _canvasGroup.blocksRaycasts = isVisible;

        if (_disableGameObjectOnHide && _panel.gameObject != gameObject)
        {
            _panel.gameObject.SetActive(isVisible);
        }
    }

    public override void SetInteractable(bool isInteractable)
    {
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.blocksRaycasts = isInteractable;
    }

    protected override void OnInitialize()
    {
        SubscribeOnEvents();
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

    private void SubscribeOnEvents()
    {
        SubscribeOnEvents(default);
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
            WaitForClicksAsync(_playButton, RaisePlayClicked, token),
            WaitForClicksAsync(_settingsButton, RaiseSettingsClicked, token),
            WaitForClicksAsync(_exitButton, RaiseExitClicked, token));
    }

    private static async UniTask WaitForClicksAsync(Button button, Action onClick, CancellationToken token)
    {
        try
        {
            await foreach (var _ in button.OnClickAsAsyncEnumerable(token))
            {
                onClick?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
}