using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

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

        public override UniTask RequestBackAsync()
        {
            return model.RequestBackAsync();
        }

        public override UniTask RequestApplyAsync()
        {
            return model.RequestApplyAsync();
        }

        protected override void OnInitialize()
        {
            view.BackClicked.Subscribe(HandleBackClicked);
            view.ApplyClicked.Subscribe(HandleApplyClicked);

            model.VisibilityChanged += HandleVisibilityChanged;
            model.BackRequested.Subscribe(HandleBackRequested);
            model.ApplyRequested.Subscribe(HandleApplyRequested);

            view.SetVisible(model.IsVisible);
        }

        protected override ValueTask OnInitializeAsync(CancellationToken token)
        {
            view.BackClicked.Subscribe(HandleBackClicked);
            view.ApplyClicked.Subscribe(HandleApplyClicked);

            model.VisibilityChanged += HandleVisibilityChanged;
            model.BackRequested.Subscribe(HandleBackRequested);
            model.ApplyRequested.Subscribe(HandleApplyRequested);

            view.SetVisible(model.IsVisible);

            return default;
        }

        protected override void OnDispose()
        {
            view.BackClicked.Unsubscribe(HandleBackClicked);
            view.ApplyClicked.Unsubscribe(HandleApplyClicked);

            model.VisibilityChanged -= HandleVisibilityChanged;
            model.BackRequested.Unsubscribe(HandleBackRequested);
            model.ApplyRequested.Unsubscribe(HandleApplyRequested);
        }

        protected override ValueTask OnDisposeAsync(CancellationToken token)
        {
            view.BackClicked.Unsubscribe(HandleBackClicked);
            view.ApplyClicked.Unsubscribe(HandleApplyClicked);

            model.VisibilityChanged -= HandleVisibilityChanged;
            model.BackRequested.Unsubscribe(HandleBackRequested);
            model.ApplyRequested.Unsubscribe(HandleApplyRequested);

            return default;
        }

        private UniTask HandleBackClicked()
        {
            return model.RequestBackAsync();
        }

        private UniTask HandleApplyClicked()
        {
            return model.RequestApplyAsync();
        }

        private void HandleVisibilityChanged(bool isVisible)
        {
            view.SetVisible(isVisible);
        }

        private UniTask HandleBackRequested()
        {
            return NotifyBackRequestedAsync();
        }

        private UniTask HandleApplyRequested()
        {
            return NotifyApplyRequestedAsync();
        }

    }
}
