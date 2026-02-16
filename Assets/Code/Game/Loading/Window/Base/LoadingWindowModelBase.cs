using System;
using System.Threading;
using System.Threading.Tasks;
using MVP;

namespace Code.Game.Loading.Window
{
public abstract class LoadingWindowModelBase : Model
{
    public event Action<float> ProgressChanged;
    public event Action<bool> VisibilityChanged;

    public abstract float Progress { get; }
    public abstract bool IsVisible { get; }
    public abstract int TotalSteps { get; }
    public abstract int CompletedSteps { get; }
    public abstract void ReportProgress(float progress);
    public abstract void Show();
    public abstract void Hide();
    public abstract void BeginSteps(int totalSteps);
    public abstract void CompleteStep();

    protected void NotifyProgressChanged(float progress)
    {
        ProgressChanged?.Invoke(progress);
    }

    protected void NotifyVisibilityChanged(bool isVisible)
    {
        VisibilityChanged?.Invoke(isVisible);
    }
}
}