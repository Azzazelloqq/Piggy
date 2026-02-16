using System;
using System.Threading;
using System.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuExitConfirmPresenterBase
    : Presenter<MainMenuExitConfirmViewBase, MainMenuExitConfirmModelBase>
{
    public event Action Confirmed;
    public event Action Canceled;

    protected MainMenuExitConfirmPresenterBase(
        MainMenuExitConfirmViewBase view,
        MainMenuExitConfirmModelBase model)
        : base(view, model)
    {
    }

    public abstract void Show();
    public abstract void Hide();

    public abstract void ConfirmExit();
    public abstract void CancelExit();

    protected void NotifyConfirmed()
    {
        Confirmed?.Invoke();
    }

    protected void NotifyCanceled()
    {
        Canceled?.Invoke();
    }
}
}