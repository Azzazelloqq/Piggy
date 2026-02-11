using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Code.Game.Loading.Window
{
public sealed class LoadingWindowPresenter : LoadingWindowPresenterBase
{
    public LoadingWindowPresenter(LoadingWindowViewBase view, LoadingWindowModelBase model)
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

    public override async UniTask ShowAsync(CancellationToken token)
    {
        await view.ShowAsync(token);
        model.Show();
    }

    public override async UniTask HideAsync(CancellationToken token)
    {
        await view.HideAsync(token);
        model.Hide();
    }

    public override void ReportProgress(float progress)
    {
        model.ReportProgress(progress);
    }

    public override void BeginSteps(int totalSteps)
    {
        model.BeginSteps(totalSteps);
    }

    public override void CompleteStep()
    {
        model.CompleteStep();
    }

    protected override void OnInitialize()
    {
        model.ProgressChanged += HandleProgressChanged;
        model.VisibilityChanged += HandleVisibilityChanged;

        view.SetProgress(model.Progress);
        view.SetVisible(model.IsVisible);
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        OnInitialize();
        return default;
    }

    protected override void OnDispose()
    {
        model.ProgressChanged -= HandleProgressChanged;
        model.VisibilityChanged -= HandleVisibilityChanged;
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        return default;
    }

    private void HandleProgressChanged(float progress)
    {
        view.SetProgress(progress);
    }

    private void HandleVisibilityChanged(bool isVisible)
    {
        view.SetVisible(isVisible);
    }
}
}