using Code.Config.Pages.CharactersPage;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class CharacterStatRowPresenterBase
    : Presenter<CharacterStatRowViewBase, CharacterStatRowModelBase>
{
    protected CharacterStatRowPresenterBase(CharacterStatRowViewBase view, CharacterStatRowModelBase model)
        : base(view, model)
    {
    }

    public abstract AsyncEvent<CharacterStatType> IncreaseRequested { get; }
    public abstract AsyncEvent<CharacterStatType> DecreaseRequested { get; }
    public abstract CharacterStatRowData Data { get; }

    public abstract void SetData(CharacterStatRowData data);
    public abstract void SetInteractable(bool isInteractable);
    public abstract UniTask RequestIncreaseAsync();
    public abstract UniTask RequestDecreaseAsync();
}
}
