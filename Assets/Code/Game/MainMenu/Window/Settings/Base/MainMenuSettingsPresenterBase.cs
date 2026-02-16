using System;
using System.Threading;
using System.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
    public abstract class MainMenuSettingsPresenterBase
        : Presenter<MainMenuSettingsViewBase, MainMenuSettingsModelBase>,
            IMainMenuPanelPresenter
    {
        public event Action BackRequested;
        public event Action ApplyRequested;

        protected MainMenuSettingsPresenterBase(MainMenuSettingsViewBase view, MainMenuSettingsModelBase model)
            : base(view, model)
        {
        }

        public abstract void Show();
        public abstract void Hide();

        public abstract void RequestBack();
        public abstract void RequestApply();

        protected void NotifyBackRequested()
        {
            BackRequested?.Invoke();
        }

        protected void NotifyApplyRequested()
        {
            ApplyRequested?.Invoke();
        }
    }
}
