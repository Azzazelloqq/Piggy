using System;
using System.Collections.Generic;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class GameProfilesModelBase : Model
{
    public abstract event Action<bool> VisibilityChanged;
    public abstract event Action<IReadOnlyList<GameProfilesSlotData>> SlotsChanged;

    public abstract AsyncEvent BackRequested { get; }
    public abstract AsyncEvent<GameProfilesSlotData> SlotSelected { get; }

    public abstract bool IsVisible { get; }
    public abstract IReadOnlyList<GameProfilesSlotData> Slots { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract void RefreshSlots();
    public abstract UniTask RequestBackAsync();
    public abstract UniTask RequestSlotSelectionAsync(GameProfilesSlotData slot);
}
}
