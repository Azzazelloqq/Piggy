using System;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuExitConfirmModelBase : Model
{
    public event Action<bool> VisibilityChanged;
    public AsyncEvent ConfirmRequested { get; } = new AsyncEvent();
    public AsyncEvent CancelRequested { get; } = new AsyncEvent();

    public abstract bool IsVisible { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract UniTask RequestConfirmAsync();
    public abstract UniTask RequestCancelAsync();

    protected void NotifyVisibilityChanged(bool isVisible)
    {
        VisibilityChanged?.Invoke(isVisible);
    }

    protected UniTask NotifyConfirmRequestedAsync()
    {
        return ConfirmRequested.InvokeAsync();
    }

    protected UniTask NotifyCancelRequestedAsync()
    {
        return CancelRequested.InvokeAsync();
    }
}
}