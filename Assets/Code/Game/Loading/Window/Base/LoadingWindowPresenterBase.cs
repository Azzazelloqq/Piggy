using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.Loading.Window
{
public abstract class LoadingWindowPresenterBase
    : Presenter<LoadingWindowViewBase, LoadingWindowModelBase>
{
    protected LoadingWindowPresenterBase(
        LoadingWindowViewBase view,
        LoadingWindowModelBase model)
        : base(view, model)
    {
    }

    public abstract void Show();
    public abstract void Hide();
    public abstract UniTask ShowAsync(CancellationToken token);
    public abstract UniTask HideAsync(CancellationToken token);
    public abstract void ReportProgress(float progress);
    public abstract void BeginSteps(int totalSteps);
    public abstract void CompleteStep();
}
}