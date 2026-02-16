using System.Threading;
using System.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
    public sealed class MainMenuSettingsPresenter : MainMenuSettingsPresenterBase
    {
        public MainMenuSettingsPresenter(MainMenuSettingsViewBase view, MainMenuSettingsModelBase model)
            : base(view, model)
        {
        }

        public override void Show()
        {
            model.Show();
        }

        public override void Hide()
        {
            model.Hide();
        }

        public override void RequestBack()
        {
            model.RequestBack();
        }

        public override void RequestApply()
        {
            model.RequestApply();
        }

        protected override void OnInitialize()
        {
            view.BackClicked += HandleBackClicked;
            view.ApplyClicked += HandleApplyClicked;

            model.VisibilityChanged += HandleVisibilityChanged;
            model.BackRequested += HandleBackRequested;
            model.ApplyRequested += HandleApplyRequested;

            view.SetVisible(model.IsVisible);
        }

        protected override ValueTask OnInitializeAsync(CancellationToken token)
        {
            view.BackClicked += HandleBackClicked;
            view.ApplyClicked += HandleApplyClicked;

            model.VisibilityChanged += HandleVisibilityChanged;
            model.BackRequested += HandleBackRequested;
            model.ApplyRequested += HandleApplyRequested;

            view.SetVisible(model.IsVisible);

            return default;
        }

        protected override void OnDispose()
        {
            view.BackClicked -= HandleBackClicked;
            view.ApplyClicked -= HandleApplyClicked;

            model.VisibilityChanged -= HandleVisibilityChanged;
            model.BackRequested -= HandleBackRequested;
            model.ApplyRequested -= HandleApplyRequested;
        }

        protected override ValueTask OnDisposeAsync(CancellationToken token)
        {
            view.BackClicked -= HandleBackClicked;
            view.ApplyClicked -= HandleApplyClicked;

            model.VisibilityChanged -= HandleVisibilityChanged;
            model.BackRequested -= HandleBackRequested;
            model.ApplyRequested -= HandleApplyRequested;

            return default;
        }

        private void HandleBackClicked()
        {
            model.RequestBack();
        }

        private void HandleApplyClicked()
        {
            model.RequestApply();
        }

        private void HandleVisibilityChanged(bool isVisible)
        {
            view.SetVisible(isVisible);
        }

        private void HandleBackRequested()
        {
            NotifyBackRequested();
        }

        private void HandleApplyRequested()
        {
            NotifyApplyRequested();
        }

    }
}
