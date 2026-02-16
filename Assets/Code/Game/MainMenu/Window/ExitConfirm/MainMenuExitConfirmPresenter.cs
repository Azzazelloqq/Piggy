using System.Threading;
using System.Threading.Tasks;

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

    public override void ConfirmExit()
    {
        model.RequestConfirm();
    }

    public override void CancelExit()
    {
        model.RequestCancel();
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

    private void HandleConfirmClicked()
    {
        model.RequestConfirm();
    }

    private void HandleCancelClicked()
    {
        model.RequestCancel();
    }

    private void HandleVisibilityChanged(bool isVisible)
    {
        view.SetVisible(isVisible);
    }

    private void HandleConfirmRequested()
    {
        NotifyConfirmed();
    }

    private void HandleCancelRequested()
    {
        NotifyCanceled();
    }

    private void SubscribeOnEvents()
    {
        view.ConfirmClicked += HandleConfirmClicked;
        view.CancelClicked += HandleCancelClicked;

        model.VisibilityChanged += HandleVisibilityChanged;
        model.ConfirmRequested += HandleConfirmRequested;
        model.CancelRequested += HandleCancelRequested;
    }

    private void UnsubscribeOnEvents()
    {
        view.ConfirmClicked -= HandleConfirmClicked;
        view.CancelClicked -= HandleCancelClicked;

        model.VisibilityChanged -= HandleVisibilityChanged;
        model.ConfirmRequested -= HandleConfirmRequested;
        model.CancelRequested -= HandleCancelRequested;
    }
}
}