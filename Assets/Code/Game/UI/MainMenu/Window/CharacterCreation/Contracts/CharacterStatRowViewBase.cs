using Code.Game.Async;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class CharacterStatRowViewBase
    : ViewMonoBehaviour<CharacterStatRowPresenterBase>
{
    public abstract AsyncEvent IncrementClicked { get; }
    public abstract AsyncEvent DecrementClicked { get; }
    public abstract void SetData(CharacterStatRowData data);
    public abstract void SetInteractable(bool isInteractable);
}
}
