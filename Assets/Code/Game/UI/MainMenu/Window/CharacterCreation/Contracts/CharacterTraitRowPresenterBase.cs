using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class CharacterTraitRowPresenterBase
    : Presenter<CharacterTraitRowViewBase, CharacterTraitRowModelBase>
{
    protected CharacterTraitRowPresenterBase(CharacterTraitRowViewBase view, CharacterTraitRowModelBase model)
        : base(view, model)
    {
    }

    public abstract AsyncEvent<CharacterTraitSelection> SelectionChanged { get; }
    public abstract CharacterTraitRowData Data { get; }

    public abstract void SetData(CharacterTraitRowData data);
    public abstract void SetInteractable(bool isInteractable);
    public abstract UniTask RequestToggleAsync(bool isSelected);
}
}
