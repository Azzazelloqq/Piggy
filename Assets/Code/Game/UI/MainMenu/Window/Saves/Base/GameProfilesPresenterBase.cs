using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class GameProfilesPresenterBase
    : Presenter<GameProfilesViewBase, GameProfilesModelBase>
{
    protected GameProfilesPresenterBase(GameProfilesViewBase view, GameProfilesModelBase model) : base(view, model)
    {
    }

    public abstract AsyncEvent BackRequested { get; }
    public abstract AsyncEvent<GameProfilesSlotData> SlotSelected { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract UniTask RequestBackAsync();
    public abstract UniTask RequestSlotSelectionAsync(GameProfilesSlotData slot);
}
}
