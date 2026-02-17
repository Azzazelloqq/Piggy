using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuExitConfirmPresenterBase
    : Presenter<MainMenuExitConfirmViewBase, MainMenuExitConfirmModelBase>
{
    public AsyncEvent Confirmed { get; } = new AsyncEvent();
    public AsyncEvent Canceled { get; } = new AsyncEvent();

    protected MainMenuExitConfirmPresenterBase(
        MainMenuExitConfirmViewBase view,
        MainMenuExitConfirmModelBase model)
        : base(view, model)
    {
    }

    public abstract void Show();
    public abstract void Hide();

    public abstract UniTask ConfirmExitAsync();
    public abstract UniTask CancelExitAsync();

    protected UniTask NotifyConfirmedAsync()
    {
        return Confirmed.InvokeAsync();
    }

    protected UniTask NotifyCanceledAsync()
    {
        return Canceled.InvokeAsync();
    }
}
}