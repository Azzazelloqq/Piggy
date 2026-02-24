using System;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class GameProfilesDeleteConfirmModelBase : Model
{
    public abstract event Action<bool> VisibilityChanged;
    public abstract AsyncEvent ConfirmRequested { get; }
    public abstract AsyncEvent CancelRequested { get; }

    public abstract bool IsVisible { get; }
    public abstract void Show();
    public abstract void Hide();
    public abstract UniTask RequestConfirmAsync();
    public abstract UniTask RequestCancelAsync();
}
}
