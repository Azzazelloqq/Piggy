using System;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuSettingsModelBase : Model
{
    public event Action<bool> VisibilityChanged;
    public AsyncEvent BackRequested { get; } = new AsyncEvent();
    public AsyncEvent ApplyRequested { get; } = new AsyncEvent();

    public abstract bool IsVisible { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract UniTask RequestBackAsync();
    public abstract UniTask RequestApplyAsync();

    protected void NotifyVisibilityChanged(bool isVisible)
    {
        VisibilityChanged?.Invoke(isVisible);
    }

    protected UniTask NotifyBackRequestedAsync()
    {
        return BackRequested.InvokeAsync();
    }

    protected UniTask NotifyApplyRequestedAsync()
    {
        return ApplyRequested.InvokeAsync();
    }
}
}