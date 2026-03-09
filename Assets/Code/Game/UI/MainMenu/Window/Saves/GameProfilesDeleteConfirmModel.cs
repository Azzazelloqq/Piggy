using System;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesDeleteConfirmModel : GameProfilesDeleteConfirmModelBase
{
    private bool _isVisible;

    public override event Action<bool> VisibilityChanged;
    public override AsyncEvent ConfirmRequested { get; } = new();
    public override AsyncEvent CancelRequested { get; } = new();

    public override bool IsVisible => _isVisible;

    public override void Show()
    {
        if (_isVisible)
        {
            return;
        }

        _isVisible = true;
        VisibilityChanged?.Invoke(true);
    }

    public override void Hide()
    {
        if (!_isVisible)
        {
            return;
        }

        _isVisible = false;
        VisibilityChanged?.Invoke(false);
    }

    public override UniTask RequestConfirmAsync()
    {
        if (!_isVisible)
        {
            return UniTask.CompletedTask;
        }

        return ConfirmRequested.InvokeAsync();
    }

    public override UniTask RequestCancelAsync()
    {
        if (!_isVisible)
        {
            return UniTask.CompletedTask;
        }

        return CancelRequested.InvokeAsync();
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
