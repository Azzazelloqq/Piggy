using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class GameProfilesSlotPresenterBase
    : Presenter<GameProfilesSlotViewBase, GameProfilesSlotModelBase>
{
    protected GameProfilesSlotPresenterBase(GameProfilesSlotViewBase view, GameProfilesSlotModelBase model)
        : base(view, model)
    {
    }

    public abstract AsyncEvent<GameProfilesSlotData> Selected { get; }
    public abstract GameProfilesSlotData Data { get; }
    public abstract void SetData(GameProfilesSlotData data);
    public abstract void SetInteractable(bool isInteractable);
    public abstract UniTask RequestSelectAsync();
}
}
