using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
    public abstract class MainMenuSettingsPresenterBase
        : Presenter<MainMenuSettingsViewBase, MainMenuSettingsModelBase>
    {
        public AsyncEvent BackRequested { get; } = new AsyncEvent();
        public AsyncEvent ApplyRequested { get; } = new AsyncEvent();

        protected MainMenuSettingsPresenterBase(MainMenuSettingsViewBase view, MainMenuSettingsModelBase model)
            : base(view, model)
        {
        }

        public abstract void Show();
        public abstract void Hide();

        public abstract UniTask RequestBackAsync();
        public abstract UniTask RequestApplyAsync();

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
