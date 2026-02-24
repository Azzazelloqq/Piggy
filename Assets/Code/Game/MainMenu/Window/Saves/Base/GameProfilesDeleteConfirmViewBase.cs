using Code.Game.Async;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class GameProfilesDeleteConfirmViewBase
    : ViewMonoBehaviour<GameProfilesDeleteConfirmPresenterBase>
{
    public abstract AsyncEvent ConfirmClicked { get; }
    public abstract AsyncEvent CancelClicked { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);
}
}
