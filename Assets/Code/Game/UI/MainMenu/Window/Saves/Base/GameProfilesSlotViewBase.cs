using Code.Game.Async;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class GameProfilesSlotViewBase
    : ViewMonoBehaviour<GameProfilesSlotPresenterBase>
{
    public abstract AsyncEvent Clicked { get; }
    public abstract AsyncEvent DeleteClicked { get; }
    public abstract void SetData(GameProfilesSlotData data);
    public abstract void SetInteractable(bool isInteractable);
}
}
