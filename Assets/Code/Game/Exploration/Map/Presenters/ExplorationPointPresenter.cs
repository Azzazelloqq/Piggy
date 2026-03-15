using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationPointPresenter : ExplorationPointPresenterBase
{
    public ExplorationPointPresenter(ExplorationPointViewBase view, ExplorationPointModelBase model)
        : base(view, model)
    {
    }

    public override AsyncEvent<string> Selected { get; } = new();
    public override string PointId => model.PointId;

    public override void RefreshView()
    {
        view.SetDisplayName(model.Data.DisplayName);
        view.SetEntityType(model.Data.EntityType);
        view.SetVisible(model.IsVisible);
        view.SetInteractable(model.IsInteractable);
        view.SetSelected(model.IsSelected);
    }

    public override void SetSelected(bool isSelected)
    {
        model.SetSelected(isSelected);
        view.SetSelected(model.IsSelected);
    }

    public override void SetMovementBlocked(bool isBlocked)
    {
        model.SetMovementBlocked(isBlocked);
        view.SetInteractable(model.IsInteractable);
    }

    protected override void OnInitialize()
    {
        view.Clicked.Subscribe(HandleClicked);
        RefreshView();
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        view.Clicked.Subscribe(HandleClicked);
        RefreshView();
        return default;
    }

    protected override void OnDispose()
    {
        view.Clicked.Unsubscribe(HandleClicked);
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        view.Clicked.Unsubscribe(HandleClicked);
        return default;
    }

    private async UniTask HandleClicked()
    {
        if (!model.CanRequestSelection())
        {
            return;
        }

        await Selected.InvokeAsync(model.PointId);
    }
}
}
