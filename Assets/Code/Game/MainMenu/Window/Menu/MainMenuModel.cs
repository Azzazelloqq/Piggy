using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuModel : MainMenuModelBase
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

    public override UniTask RequestPlayAsync()
    {
        if (!_isVisible)
        {
            return UniTask.CompletedTask;
        }

        return NotifyPlayRequestedAsync();
    }

    public override UniTask RequestSettingsAsync()
    {
        if (!_isVisible)
        {
            return UniTask.CompletedTask;
        }

        return NotifySettingsRequestedAsync();
    }

    public override UniTask RequestExitAsync()
    {
        if (!_isVisible)
        {
            return UniTask.CompletedTask;
        }

        return NotifyExitRequestedAsync();
    }

    protected override void OnInitialize()
    {
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
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