using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

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

        public override UniTask RequestBackAsync()
        {
            if (!_isVisible)
            {
                return UniTask.CompletedTask;
            }

            return NotifyBackRequestedAsync();
        }

        public override UniTask RequestApplyAsync()
        {
            if (!_isVisible)
            {
                return UniTask.CompletedTask;
            }

            return NotifyApplyRequestedAsync();
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
