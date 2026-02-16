using System;
using System.Threading;
using System.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuExitConfirmModelBase : Model
{
    public event Action<bool> VisibilityChanged;
    public event Action ConfirmRequested;
    public event Action CancelRequested;

    public abstract bool IsVisible { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract void RequestConfirm();
    public abstract void RequestCancel();

    protected void NotifyVisibilityChanged(bool isVisible)
    {
        VisibilityChanged?.Invoke(isVisible);
    }

    protected void NotifyConfirmRequested()
    {
        ConfirmRequested?.Invoke();
    }

    protected void NotifyCancelRequested()
    {
        CancelRequested?.Invoke();
    }
}
}