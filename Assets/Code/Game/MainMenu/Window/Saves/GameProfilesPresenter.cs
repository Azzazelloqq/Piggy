using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesPresenter : GameProfilesPresenterBase
{
    public override AsyncEvent BackRequested { get; } = new();
    public override AsyncEvent<GameProfilesSlotData> SlotSelected { get; } = new();
    private readonly List<GameProfilesSlotPresenter> _slotPresenters = new();

    public GameProfilesPresenter(GameProfilesViewBase view, GameProfilesModelBase model)
        : base(view, model)
    {
    }

    public override void Show()
    {
        model.RefreshSlots();
        SyncSlots();
        model.Show();
        view.SetVisible(model.IsVisible);
    }

    public override void Hide()
    {
        model.Hide();
        view.SetVisible(model.IsVisible);
    }

    public override UniTask RequestBackAsync()
    {
        return model.RequestBackAsync();
    }

    public override UniTask RequestSlotSelectionAsync(GameProfilesSlotData slot)
    {
        return model.RequestSlotSelectionAsync(slot);
    }

    protected override void OnInitialize()
    {
        SubscribeOnEvents();
        model.RefreshSlots();
        view.SetVisible(model.IsVisible);
        SyncSlots();
    }

    protected override async ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeOnEvents();
        model.RefreshSlots();
        view.SetVisible(model.IsVisible);
        await SyncSlotsAsync(token);
    }

    protected override void OnDispose()
    {
        UnsubscribeOnEvents();
        DisposeSlotPresenters();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeOnEvents();
        DisposeSlotPresenters();
        return default;
    }

    private UniTask HandleBackClicked()
    {
        return model.RequestBackAsync();
    }

    private UniTask HandleBackRequested()
    {
        return BackRequested.InvokeAsync();
    }

    private UniTask HandleSlotSelected(GameProfilesSlotData slot)
    {
        return model.RequestSlotSelectionAsync(slot);
    }

    private UniTask HandleModelSlotSelected(GameProfilesSlotData slot)
    {
        return SlotSelected.InvokeAsync(slot);
    }

    private void SubscribeOnEvents()
    {
        view.BackClicked.Subscribe(HandleBackClicked);
        model.BackRequested.Subscribe(HandleBackRequested);
        model.SlotSelected.Subscribe(HandleModelSlotSelected);
    }

    private void UnsubscribeOnEvents()
    {
        view.BackClicked.Unsubscribe(HandleBackClicked);
        model.BackRequested.Unsubscribe(HandleBackRequested);
        model.SlotSelected.Unsubscribe(HandleModelSlotSelected);
    }

    private void SyncSlots()
    {
        var slots = model.Slots;
        var slotViews = view.EnsureSlotViews(slots.Count);
        EnsureSlotPresenters(slotViews);

        for (var i = 0; i < slots.Count; i++)
        {
            _slotPresenters[i].SetData(slots[i]);
        }
    }

    private async UniTask SyncSlotsAsync(CancellationToken token)
    {
        var slots = model.Slots;
        var slotViews = view.EnsureSlotViews(slots.Count);
        await EnsureSlotPresentersAsync(slotViews, token);

        for (var i = 0; i < slots.Count; i++)
        {
            _slotPresenters[i].SetData(slots[i]);
        }
    }

    private void EnsureSlotPresenters(IReadOnlyList<GameProfilesSlotViewBase> slotViews)
    {
        for (var i = _slotPresenters.Count; i < slotViews.Count; i++)
        {
            var presenter = new GameProfilesSlotPresenter(slotViews[i], new GameProfilesSlotModel());
            presenter.Selected.Subscribe(HandleSlotSelected);
            presenter.Initialize();
            _slotPresenters.Add(presenter);
        }

        for (var i = _slotPresenters.Count - 1; i >= slotViews.Count; i--)
        {
            var presenter = _slotPresenters[i];
            presenter.Selected.Unsubscribe(HandleSlotSelected);
            presenter.Dispose();
            _slotPresenters.RemoveAt(i);
        }
    }

    private async UniTask EnsureSlotPresentersAsync(
        IReadOnlyList<GameProfilesSlotViewBase> slotViews,
        CancellationToken token)
    {
        for (var i = _slotPresenters.Count; i < slotViews.Count; i++)
        {
            var presenter = new GameProfilesSlotPresenter(slotViews[i], new GameProfilesSlotModel());
            presenter.Selected.Subscribe(HandleSlotSelected);
            await presenter.InitializeAsync(token);
            _slotPresenters.Add(presenter);
        }

        for (var i = _slotPresenters.Count - 1; i >= slotViews.Count; i--)
        {
            var presenter = _slotPresenters[i];
            presenter.Selected.Unsubscribe(HandleSlotSelected);
            presenter.Dispose();
            _slotPresenters.RemoveAt(i);
        }
    }

    private void DisposeSlotPresenters()
    {
        for (var i = 0; i < _slotPresenters.Count; i++)
        {
            _slotPresenters[i].Selected.Unsubscribe(HandleSlotSelected);
            _slotPresenters[i].Dispose();
        }

        _slotPresenters.Clear();
    }
}
}
