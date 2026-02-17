using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuExitConfirmPresenter : MainMenuExitConfirmPresenterBase
{
    public MainMenuExitConfirmPresenter(
        MainMenuExitConfirmViewBase view,
        MainMenuExitConfirmModelBase model)
        : base(view, model)
    {
    }

    public override void Show()
    {
        model.Show();
    }

    public override void Hide()
    {
        model.Hide();
    }

    public override UniTask ConfirmExitAsync()
    {
        return model.RequestConfirmAsync();
    }

    public override UniTask CancelExitAsync()
    {
        return model.RequestCancelAsync();
    }

    protected override void OnInitialize()
    {
        SubscribeOnEvents();

        view.SetVisible(model.IsVisible);
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeOnEvents();
        view.SetVisible(model.IsVisible);

        return default;
    }

    protected override void OnDispose()
    {
        UnsubscribeOnEvents();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeOnEvents();
        return default;
    }

    private UniTask HandleConfirmClicked()
    {
        return model.RequestConfirmAsync();
    }

    private UniTask HandleCancelClicked()
    {
        return model.RequestCancelAsync();
    }

    private void HandleVisibilityChanged(bool isVisible)
    {
        view.SetVisible(isVisible);
    }

    private UniTask HandleConfirmRequested()
    {
        return NotifyConfirmedAsync();
    }

    private UniTask HandleCancelRequested()
    {
        return NotifyCanceledAsync();
    }

    private void SubscribeOnEvents()
    {
        view.ConfirmClicked.Subscribe(HandleConfirmClicked);
        view.CancelClicked.Subscribe(HandleCancelClicked);

        model.VisibilityChanged += HandleVisibilityChanged;
        model.ConfirmRequested.Subscribe(HandleConfirmRequested);
        model.CancelRequested.Subscribe(HandleCancelRequested);
    }

    private void UnsubscribeOnEvents()
    {
        view.ConfirmClicked.Unsubscribe(HandleConfirmClicked);
        view.CancelClicked.Unsubscribe(HandleCancelClicked);

        model.VisibilityChanged -= HandleVisibilityChanged;
        model.ConfirmRequested.Unsubscribe(HandleConfirmRequested);
        model.CancelRequested.Unsubscribe(HandleCancelRequested);
    }
}
}