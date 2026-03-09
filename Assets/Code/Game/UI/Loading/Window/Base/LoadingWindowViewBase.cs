using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.Loading.Window
{
public abstract class LoadingWindowViewBase : ViewMonoBehaviour<LoadingWindowPresenterBase>
{
    public abstract void SetVisible(bool isVisible);
    public abstract void SetProgress(float progress);

    public virtual UniTask ShowAsync(CancellationToken token)
    {
        SetVisible(true);
        return UniTask.CompletedTask;
    }

    public virtual UniTask HideAsync(CancellationToken token)
    {
        SetVisible(false);
        return UniTask.CompletedTask;
    }
}
}