using System;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuModelBase : Model
{
    public event Action<bool> VisibilityChanged;
    public AsyncEvent PlayRequested { get; } = new AsyncEvent();
    public AsyncEvent SettingsRequested { get; } = new AsyncEvent();
    public AsyncEvent ExitRequested { get; } = new AsyncEvent();

    public abstract bool IsVisible { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract UniTask RequestPlayAsync();
    public abstract UniTask RequestSettingsAsync();
    public abstract UniTask RequestExitAsync();

    protected void NotifyVisibilityChanged(bool isVisible)
    {
        VisibilityChanged?.Invoke(isVisible);
    }

    protected UniTask NotifyPlayRequestedAsync()
    {
        return PlayRequested.InvokeAsync();
    }

    protected UniTask NotifySettingsRequestedAsync()
    {
        return SettingsRequested.InvokeAsync();
    }

    protected UniTask NotifyExitRequestedAsync()
    {
        return ExitRequested.InvokeAsync();
    }
}
}