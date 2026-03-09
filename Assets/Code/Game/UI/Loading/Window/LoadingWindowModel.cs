using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Loading.Window
{
public sealed class LoadingWindowModel : LoadingWindowModelBase
{
    public override float Progress => _totalSteps > 0
        ? Mathf.Clamp01((float)_completedSteps / _totalSteps)
        : _manualProgress;

    public override bool IsVisible => _isVisible;
    public override int TotalSteps => _totalSteps;
    public override int CompletedSteps => _completedSteps;
    
    private float _manualProgress;
    private bool _isVisible;
    private int _totalSteps;
    private int _completedSteps;

    public override void ReportProgress(float progress)
    {
        _manualProgress = Mathf.Clamp01(progress);
        NotifyProgressChanged(Progress);
    }

    public override void Show()
    {
        _isVisible = true;
        NotifyVisibilityChanged(_isVisible);
    }

    public override void Hide()
    {
        _isVisible = false;
        NotifyVisibilityChanged(_isVisible);
    }

    public override void BeginSteps(int totalSteps)
    {
        _totalSteps = Mathf.Max(0, totalSteps);
        _completedSteps = 0;
        _manualProgress = 0f;
        NotifyProgressChanged(Progress);
    }

    public override void CompleteStep()
    {
        if (_totalSteps <= 0)
        {
            return;
        }

        _completedSteps = Mathf.Min(_completedSteps + 1, _totalSteps);
        NotifyProgressChanged(Progress);
    }

    protected override void OnInitialize()
    {
        _manualProgress = 0f;
        _isVisible = false;
        _totalSteps = 0;
        _completedSteps = 0;
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        _manualProgress = 0f;
        _isVisible = false;
        _totalSteps = 0;
        _completedSteps = 0;
        
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