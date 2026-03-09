using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class GameProfilesDeleteConfirmPresenterBase
    : Presenter<GameProfilesDeleteConfirmViewBase, GameProfilesDeleteConfirmModelBase>
{
    protected GameProfilesDeleteConfirmPresenterBase(
        GameProfilesDeleteConfirmViewBase view,
        GameProfilesDeleteConfirmModelBase model)
        : base(view, model)
    {
    }

    public abstract AsyncEvent Confirmed { get; }
    public abstract AsyncEvent Canceled { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract UniTask ConfirmDeleteAsync();
    public abstract UniTask CancelDeleteAsync();
}
}
