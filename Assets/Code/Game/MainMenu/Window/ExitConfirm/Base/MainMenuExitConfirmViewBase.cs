using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MVP;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuExitConfirmViewBase
    : ViewMonoBehaviour<MainMenuExitConfirmPresenterBase>
{
    public event Action ConfirmClicked;
    public event Action CancelClicked;

    public abstract RectTransform Panel { get; }
    public abstract IReadOnlyList<RectTransform> AnimatedElements { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);

    protected void RaiseConfirmClicked()
    {
        ConfirmClicked?.Invoke();
    }

    protected void RaiseCancelClicked()
    {
        CancelClicked?.Invoke();
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