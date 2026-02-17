using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuPresenter : MainMenuPresenterBase
{
    private readonly MainMenuSettingsPresenter _settingsPresenter;
    private readonly MainMenuExitConfirmPresenter _exitPresenter;
    private readonly MainMenuScreenTransitionPresenter _transitionPresenter;

    public AsyncEvent SettingsBackRequested { get; } = new();
    public AsyncEvent SettingsApplyRequested { get; } = new();
    public AsyncEvent ExitConfirmed { get; } = new();
    public AsyncEvent ExitCanceled { get; } = new();

    public MainMenuPresenter(MainMenuViewBase view, MainMenuModelBase model)
        : base(view, model)
    {
        var settingsViewBase = view.SettingsView;
        var exitConfirmViewBase = view.ExitConfirmView;

        var settingsModel = new MainMenuSettingsModel();
        var exitModel = new MainMenuExitConfirmModel();

        _settingsPresenter = new MainMenuSettingsPresenter(settingsViewBase, settingsModel);
        _exitPresenter = new MainMenuExitConfirmPresenter(exitConfirmViewBase, exitModel);

        compositeDisposable.AddDisposable(_settingsPresenter);
        compositeDisposable.AddDisposable(_exitPresenter);

        var menuPanel = new MainMenuScreenTransitionView.PanelHandle(
            view.Panel,
            Show,
            Hide,
            view.SetInteractable,
            view.AnimatedElements);
        var settingsPanel = new MainMenuScreenTransitionView.PanelHandle(
            settingsViewBase.Panel,
            _settingsPresenter.Show,
            _settingsPresenter.Hide,
            settingsViewBase.SetInteractable,
            settingsViewBase.AnimatedElements);
        var exitPanel = new MainMenuScreenTransitionView.PanelHandle(
            exitConfirmViewBase.Panel,
            _exitPresenter.Show,
            _exitPresenter.Hide,
            exitConfirmViewBase.SetInteractable,
            exitConfirmViewBase.AnimatedElements);
        var transitionView = new MainMenuScreenTransitionView(view.Layout, menuPanel, settingsPanel, exitPanel);
        _transitionPresenter = new MainMenuScreenTransitionPresenter(
            transitionView,
            new MainMenuScreenTransitionModel());
    }

    public void ApplyScreenLayoutImmediate(MainMenuScreen screen)
    {
        _transitionPresenter.ApplyScreenLayoutImmediate(screen);
    }

    public async UniTask<bool> TryTransitionToScreenAsync(MainMenuScreen targetScreen, CancellationToken token)
    {
        return await _transitionPresenter.TryTransitionToScreenAsync(targetScreen, token);
    }

    protected override void OnInitialize()
    {
        SubscribeOnEvents();

        _settingsPresenter.Initialize();
        _exitPresenter.Initialize();

        view.SetVisible(model.IsVisible);
    }

    protected override async ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeOnEvents();

        await _settingsPresenter.InitializeAsync(token);
        await _exitPresenter.InitializeAsync(token);

        view.SetVisible(model.IsVisible);
    }

    protected override void OnDispose()
    {
        UnsubscribeOneEvents();

        _settingsPresenter.Dispose();
        _exitPresenter.Dispose();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeOneEvents();

        _settingsPresenter.Dispose();
        _exitPresenter.Dispose();

        return default;
    }

    public override void Show()
    {
        model.Show();
    }

    public override void Hide()
    {
        model.Hide();
    }

    public override UniTask RequestPlayAsync()
    {
        return model.RequestPlayAsync();
    }

    public override UniTask RequestSettingsAsync()
    {
        return model.RequestSettingsAsync();
    }

    public override UniTask RequestExitAsync()
    {
        return model.RequestExitAsync();
    }

    private UniTask HandlePlayClicked()
    {
        return model.RequestPlayAsync();
    }

    private UniTask HandleSettingsClicked()
    {
        return model.RequestSettingsAsync();
    }

    private UniTask HandleExitClicked()
    {
        return model.RequestExitAsync();
    }

    private void HandleVisibilityChanged(bool isVisible)
    {
        view.SetVisible(isVisible);
    }

    private UniTask HandlePlayRequested()
    {
        return NotifyPlayRequestedAsync();
    }

    private UniTask HandleSettingsRequested()
    {
        return NotifySettingsRequestedAsync();
    }

    private UniTask HandleExitRequested()
    {
        return NotifyExitRequestedAsync();
    }

    private UniTask HandleSettingsBackRequested()
    {
        return SettingsBackRequested.InvokeAsync();
    }

    private UniTask HandleSettingsApplyRequested()
    {
        return SettingsApplyRequested.InvokeAsync();
    }

    private UniTask HandleExitConfirmed()
    {
        return ExitConfirmed.InvokeAsync();
    }

    private UniTask HandleExitCanceled()
    {
        return ExitCanceled.InvokeAsync();
    }

    private void SubscribeOnEvents()
    {
        view.PlayClicked.Subscribe(HandlePlayClicked);
        view.SettingsClicked.Subscribe(HandleSettingsClicked);
        view.ExitClicked.Subscribe(HandleExitClicked);

        model.VisibilityChanged += HandleVisibilityChanged;
        model.PlayRequested.Subscribe(HandlePlayRequested);
        model.SettingsRequested.Subscribe(HandleSettingsRequested);
        model.ExitRequested.Subscribe(HandleExitRequested);

        _settingsPresenter.BackRequested.Subscribe(HandleSettingsBackRequested);
        _settingsPresenter.ApplyRequested.Subscribe(HandleSettingsApplyRequested);

        _exitPresenter.Confirmed.Subscribe(HandleExitConfirmed);
        _exitPresenter.Canceled.Subscribe(HandleExitCanceled);
    }

    private void UnsubscribeOneEvents()
    {
        view.PlayClicked.Unsubscribe(HandlePlayClicked);
        view.SettingsClicked.Unsubscribe(HandleSettingsClicked);
        view.ExitClicked.Unsubscribe(HandleExitClicked);

        model.VisibilityChanged -= HandleVisibilityChanged;
        model.PlayRequested.Unsubscribe(HandlePlayRequested);
        model.SettingsRequested.Unsubscribe(HandleSettingsRequested);
        model.ExitRequested.Unsubscribe(HandleExitRequested);

        _settingsPresenter.BackRequested.Unsubscribe(HandleSettingsBackRequested);
        _settingsPresenter.ApplyRequested.Unsubscribe(HandleSettingsApplyRequested);

        _exitPresenter.Confirmed.Unsubscribe(HandleExitConfirmed);
        _exitPresenter.Canceled.Unsubscribe(HandleExitCanceled);
    }

}
}