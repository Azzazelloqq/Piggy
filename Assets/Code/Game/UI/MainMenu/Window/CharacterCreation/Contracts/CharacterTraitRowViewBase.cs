using Code.Game.Async;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class CharacterTraitRowViewBase
    : ViewMonoBehaviour<CharacterTraitRowPresenterBase>
{
    public abstract AsyncEvent<bool> Toggled { get; }
    public abstract void SetData(CharacterTraitRowData data);
    public abstract void SetInteractable(bool isInteractable);
}
}
