using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Code.Game.Saves;
using Code.Game.Saves.Profile;
using Cysharp.Threading.Tasks;
using LocalSaveSystem;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesModel : GameProfilesModelBase
{
    private const int DefaultSlotsCount = 4;

    private readonly ISaveStore _saveStore;
    private readonly List<GameProfilesSlotData> _slots = new();
    private bool _isVisible;

    public GameProfilesModel(ISaveStore saveStore)
    {
        _saveStore = saveStore ?? throw new ArgumentNullException(nameof(saveStore));
    }

    public override event Action<bool> VisibilityChanged;
    public override event Action<IReadOnlyList<GameProfilesSlotData>> SlotsChanged;

    public override AsyncEvent BackRequested { get; } = new();
    public override AsyncEvent<GameProfilesSlotData> SlotSelected { get; } = new();

    public override bool IsVisible => _isVisible;
    public override IReadOnlyList<GameProfilesSlotData> Slots => _slots;

    public override void Show()
    {
        if (_isVisible)
        {
            return;
        }

        _isVisible = true;
        VisibilityChanged?.Invoke(true);
    }

    public override void Hide()
    {
        if (!_isVisible)
        {
            return;
        }

        _isVisible = false;
        VisibilityChanged?.Invoke(false);
    }

    public override void RefreshSlots()
    {
        var profiles = _saveStore.Get(GameSaveKeys.GameProfiles);
        var saves = profiles.GameProfileSaves ?? Array.Empty<GameProfileSave>();
        var slotCount = Math.Max(DefaultSlotsCount, saves.Length);

        _slots.Clear();
        for (var i = 0; i < slotCount; i++)
        {
            if (i < saves.Length)
            {
                _slots.Add(new GameProfilesSlotData(i, true, saves[i]));
            }
            else
            {
                _slots.Add(new GameProfilesSlotData(i, false, default));
            }
        }

        SlotsChanged?.Invoke(_slots);
    }

    public override UniTask RequestBackAsync()
    {
        if (!_isVisible)
        {
            return UniTask.CompletedTask;
        }

        return BackRequested.InvokeAsync();
    }

    public override UniTask RequestSlotSelectionAsync(GameProfilesSlotData slot)
    {
        if (!_isVisible)
        {
            return UniTask.CompletedTask;
        }

        return SlotSelected.InvokeAsync(slot);
    }

    protected override void OnInitialize()
    {
        _isVisible = false;
        _slots.Clear();
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        _isVisible = false;
        _slots.Clear();
        return default;
    }

    protected override void OnDispose()
    {
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        return default;
    }
}
}
