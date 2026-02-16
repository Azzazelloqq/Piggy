using System;
using System.Threading;
using System.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuModelBase : Model
{
    public event Action<bool> VisibilityChanged;
    public event Action PlayRequested;
    public event Action SettingsRequested;
    public event Action ExitRequested;

    public abstract bool IsVisible { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract void RequestPlay();
    public abstract void RequestSettings();
    public abstract void RequestExit();

    protected void NotifyVisibilityChanged(bool isVisible)
    {
        VisibilityChanged?.Invoke(isVisible);
    }

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