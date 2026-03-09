using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesDeleteConfirmPresenter : GameProfilesDeleteConfirmPresenterBase
{
    public GameProfilesDeleteConfirmPresenter(
        GameProfilesDeleteConfirmViewBase view,
        GameProfilesDeleteConfirmModelBase model)
        : base(view, model)
    {
    }

    public override AsyncEvent Confirmed { get; } = new();
    public override AsyncEvent Canceled { get; } = new();

    public override void Show()
    {
        model.Show();
        view.SetVisible(model.IsVisible);
    }

    public override void Hide()
    {
        model.Hide();
        view.SetVisible(model.IsVisible);
    }

    public override UniTask ConfirmDeleteAsync()
    {
        return model.RequestConfirmAsync();
    }

    public override UniTask CancelDeleteAsync()
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

    private UniTask HandleConfirmRequested()
    {
        return Confirmed.InvokeAsync();
    }

    private UniTask HandleCancelRequested()
    {
        return Canceled.InvokeAsync();
    }

    private void SubscribeOnEvents()
    {
        view.ConfirmClicked.Subscribe(HandleConfirmClicked);
        view.CancelClicked.Subscribe(HandleCancelClicked);

        model.ConfirmRequested.Subscribe(HandleConfirmRequested);
        model.CancelRequested.Subscribe(HandleCancelRequested);
    }

    private void UnsubscribeOnEvents()
    {
        view.ConfirmClicked.Unsubscribe(HandleConfirmClicked);
        view.CancelClicked.Unsubscribe(HandleCancelClicked);

        model.ConfirmRequested.Unsubscribe(HandleConfirmRequested);
        model.CancelRequested.Unsubscribe(HandleCancelRequested);
    }
}
}
