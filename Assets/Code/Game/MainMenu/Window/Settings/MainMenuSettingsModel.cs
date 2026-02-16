using System.Threading;
using System.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
    public sealed class MainMenuSettingsModel : MainMenuSettingsModelBase
    {
        private bool _isVisible;

        public override bool IsVisible => _isVisible;

        public override void Show()
        {
            if (_isVisible)
            {
                return;
            }

            _isVisible = true;
            NotifyVisibilityChanged(true);
        }

        public override void Hide()
        {
            if (!_isVisible)
            {
                return;
            }

            _isVisible = false;
            NotifyVisibilityChanged(false);
        }

        public override void RequestBack()
        {
            if (!_isVisible)
            {
                return;
            }

            NotifyBackRequested();
        }

        public override void RequestApply()
        {
            if (!_isVisible)
            {
                return;
            }

            NotifyApplyRequested();
        }

        protected override void OnInitialize()
        {
            _isVisible = false;
        }

        protected override ValueTask OnInitializeAsync(CancellationToken token)
        {
            _isVisible = false;
            
            return default;
        }

        protected override void OnDispose()
        {
        }

        protected override ValueTask OnDisposeAsync(CancellationToken token)
        {
            return default;
        }
    }
}
