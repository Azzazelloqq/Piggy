using System.Threading;
using System.Threading.Tasks;

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

    public override void RequestPlay()
    {
        if (!_isVisible)
        {
            return;
        }

        NotifyPlayRequested();
    }

    public override void RequestSettings()
    {
        if (!_isVisible)
        {
            return;
        }

        NotifySettingsRequested();
    }

    public override void RequestExit()
    {
        if (!_isVisible)
        {
            return;
        }

        NotifyExitRequested();
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