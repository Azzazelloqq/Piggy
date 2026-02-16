using System;
using System.Threading;
using System.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuPresenterBase
    : Presenter<MainMenuViewBase, MainMenuModelBase>
{
    public event Action PlayRequested;
    public event Action SettingsRequested;
    public event Action ExitRequested;

    protected MainMenuPresenterBase(MainMenuViewBase view, MainMenuModelBase model)
        : base(view, model)
    {
    }

    public abstract void Show();
    public abstract void Hide();

    public abstract void RequestPlay();
    public abstract void RequestSettings();
    public abstract void RequestExit();

    protected void NotifyPlayRequested()
    {
        PlayRequested?.Invoke();
    }

    protected void NotifySettingsRequested()
    {
        SettingsRequested?.Invoke();
    }

    protected void NotifyExitRequested()
    {
        ExitRequested?.Invoke();
    }
}
}