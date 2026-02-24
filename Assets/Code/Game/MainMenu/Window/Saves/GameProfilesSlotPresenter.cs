using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesSlotPresenter : GameProfilesSlotPresenterBase
{
    public GameProfilesSlotPresenter(GameProfilesSlotViewBase view, GameProfilesSlotModelBase model)
        : base(view, model)
    {
    }

    public override AsyncEvent<GameProfilesSlotData> Selected { get; } = new();
    public override AsyncEvent<GameProfilesSlotData> DeleteRequested { get; } = new();
    public override GameProfilesSlotData Data => model.Data;

    public override void SetData(GameProfilesSlotData data)
    {
        model.SetData(data);
        view.SetData(data);
    }

    public override void SetInteractable(bool isInteractable)
    {
        view.SetInteractable(isInteractable);
    }

    public override async UniTask RequestSelectAsync()
    {
        await model.RequestSelectAsync();
        await Selected.InvokeAsync(model.Data);
    }

    public override async UniTask RequestDeleteAsync()
    {
        await model.RequestDeleteAsync();
        await DeleteRequested.InvokeAsync(model.Data);
    }

    protected override void OnInitialize()
    {
        view.Clicked.Subscribe(HandleClicked);
        view.DeleteClicked.Subscribe(HandleDeleteClicked);
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        view.Clicked.Subscribe(HandleClicked);
        view.DeleteClicked.Subscribe(HandleDeleteClicked);
        return default;
    }

    protected override void OnDispose()
    {
        view.Clicked.Unsubscribe(HandleClicked);
        view.DeleteClicked.Unsubscribe(HandleDeleteClicked);
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        view.Clicked.Unsubscribe(HandleClicked);
        view.DeleteClicked.Unsubscribe(HandleDeleteClicked);
        return default;
    }

    private UniTask HandleClicked()
    {
        return RequestSelectAsync();
    }

    private UniTask HandleDeleteClicked()
    {
        return RequestDeleteAsync();
    }
}
}
