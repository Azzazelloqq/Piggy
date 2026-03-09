using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesPresenter : GameProfilesPresenterBase
{
    public override AsyncEvent BackRequested { get; } = new();
    public override AsyncEvent<GameProfilesSlotData> SlotSelected { get; } = new();
    private readonly List<GameProfilesSlotPresenter> _slotPresenters = new();
    private readonly GameProfilesDeleteConfirmPresenter _deleteConfirmPresenter;
    private GameProfilesSlotData _pendingDeleteSlot;
    private bool _hasPendingDelete;

    public GameProfilesPresenter(GameProfilesViewBase view, GameProfilesModelBase model)
        : base(view, model)
    {
        _deleteConfirmPresenter = new GameProfilesDeleteConfirmPresenter(
            view.DeleteConfirmView,
            new GameProfilesDeleteConfirmModel());
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
        _hasPendingDelete = false;
        _deleteConfirmPresenter.Hide();
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
        _deleteConfirmPresenter.Initialize();
        model.RefreshSlots();
        view.SetVisible(model.IsVisible);
        SyncSlots();
    }

    protected override async ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeOnEvents();
        await _deleteConfirmPresenter.InitializeAsync(token);
        model.RefreshSlots();
        view.SetVisible(model.IsVisible);
        await SyncSlotsAsync(token);
    }

    protected override void OnDispose()
    {
        UnsubscribeOnEvents();
        _deleteConfirmPresenter.Dispose();
        DisposeSlotPresenters();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeOnEvents();
        _deleteConfirmPresenter.Dispose();
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

    private UniTask HandleSlotDeleteRequested(GameProfilesSlotData slot)
    {
        _pendingDeleteSlot = slot;
        _hasPendingDelete = true;
        _deleteConfirmPresenter.Show();
        return UniTask.CompletedTask;
    }

    private UniTask HandleModelSlotSelected(GameProfilesSlotData slot)
    {
        return SlotSelected.InvokeAsync(slot);
    }

    private UniTask HandleDeleteConfirmed()
    {
        if (_hasPendingDelete)
        {
            var slot = _pendingDeleteSlot;
            Debug.Log($"GameProfiles: delete save slot {slot.Index} requested.");
        }

        _hasPendingDelete = false;
        _deleteConfirmPresenter.Hide();
        return UniTask.CompletedTask;
    }

    private UniTask HandleDeleteCanceled()
    {
        _hasPendingDelete = false;
        _deleteConfirmPresenter.Hide();
        return UniTask.CompletedTask;
    }

    private void SubscribeOnEvents()
    {
        view.BackClicked.Subscribe(HandleBackClicked);
        model.BackRequested.Subscribe(HandleBackRequested);
        model.SlotSelected.Subscribe(HandleModelSlotSelected);
        _deleteConfirmPresenter.Confirmed.Subscribe(HandleDeleteConfirmed);
        _deleteConfirmPresenter.Canceled.Subscribe(HandleDeleteCanceled);
    }

    private void UnsubscribeOnEvents()
    {
        view.BackClicked.Unsubscribe(HandleBackClicked);
        model.BackRequested.Unsubscribe(HandleBackRequested);
        model.SlotSelected.Unsubscribe(HandleModelSlotSelected);
        _deleteConfirmPresenter.Confirmed.Unsubscribe(HandleDeleteConfirmed);
        _deleteConfirmPresenter.Canceled.Unsubscribe(HandleDeleteCanceled);
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
            presenter.DeleteRequested.Subscribe(HandleSlotDeleteRequested);
            presenter.Initialize();
            _slotPresenters.Add(presenter);
        }

        for (var i = _slotPresenters.Count - 1; i >= slotViews.Count; i--)
        {
            var presenter = _slotPresenters[i];
            presenter.Selected.Unsubscribe(HandleSlotSelected);
            presenter.DeleteRequested.Unsubscribe(HandleSlotDeleteRequested);
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
            presenter.DeleteRequested.Subscribe(HandleSlotDeleteRequested);
            await presenter.InitializeAsync(token);
            _slotPresenters.Add(presenter);
        }

        for (var i = _slotPresenters.Count - 1; i >= slotViews.Count; i--)
        {
            var presenter = _slotPresenters[i];
            presenter.Selected.Unsubscribe(HandleSlotSelected);
            presenter.DeleteRequested.Unsubscribe(HandleSlotDeleteRequested);
            presenter.Dispose();
            _slotPresenters.RemoveAt(i);
        }
    }

    private void DisposeSlotPresenters()
    {
        for (var i = 0; i < _slotPresenters.Count; i++)
        {
            _slotPresenters[i].Selected.Unsubscribe(HandleSlotSelected);
            _slotPresenters[i].DeleteRequested.Unsubscribe(HandleSlotDeleteRequested);
            _slotPresenters[i].Dispose();
        }

        _slotPresenters.Clear();
    }
}
}
