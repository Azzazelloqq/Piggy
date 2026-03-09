using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
    public abstract class SettingsPopupPresenterBase
        : Presenter<SettingsPopupViewBase, SettingsPopupModelBase>
    {
        public AsyncEvent BackRequested { get; } = new AsyncEvent();
        public AsyncEvent ApplyRequested { get; } = new AsyncEvent();

        protected SettingsPopupPresenterBase(SettingsPopupViewBase popupView, SettingsPopupModelBase popupModel)
            : base(popupView, popupModel)
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
