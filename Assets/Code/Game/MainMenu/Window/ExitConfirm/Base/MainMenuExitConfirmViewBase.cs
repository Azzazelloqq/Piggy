using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuExitConfirmViewBase
    : ViewMonoBehaviour<MainMenuExitConfirmPresenterBase>
{
    public AsyncEvent ConfirmClicked { get; } = new AsyncEvent();
    public AsyncEvent CancelClicked { get; } = new AsyncEvent();

    public abstract RectTransform Panel { get; }
    public abstract IReadOnlyList<RectTransform> AnimatedElements { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);

    protected UniTask RaiseConfirmClicked()
    {
        return ConfirmClicked.InvokeAsync();
    }

    protected UniTask RaiseCancelClicked()
    {
        return CancelClicked.InvokeAsync();
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