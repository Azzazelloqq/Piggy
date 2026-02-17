using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuExitConfirmModel : MainMenuExitConfirmModelBase
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

    public override UniTask RequestConfirmAsync()
    {
        if (!_isVisible)
        {
            return UniTask.CompletedTask;
        }

        return NotifyConfirmRequestedAsync();
    }

    public override UniTask RequestCancelAsync()
    {
        if (!_isVisible)
        {
            return UniTask.CompletedTask;
        }

        return NotifyCancelRequestedAsync();
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