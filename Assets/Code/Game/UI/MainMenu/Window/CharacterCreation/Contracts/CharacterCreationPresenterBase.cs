using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class CharacterCreationPresenterBase
    : Presenter<CharacterCreationViewBase, CharacterCreationModelBase>
{
    protected CharacterCreationPresenterBase(CharacterCreationViewBase view, CharacterCreationModelBase model)
        : base(view, model)
    {
    }

    public abstract AsyncEvent BackRequested { get; }
    public abstract AsyncEvent<CharacterCreationResult> CreateRequested { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract void PrepareSlot(int slotIndex);
    public abstract UniTask RequestBackAsync();
    public abstract UniTask RequestCreateAsync();
}
}
