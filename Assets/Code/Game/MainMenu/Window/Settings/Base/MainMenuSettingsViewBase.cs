using System.Collections.Generic;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuSettingsViewBase
    : ViewMonoBehaviour<MainMenuSettingsPresenterBase>
{
    public AsyncEvent BackClicked { get; } = new AsyncEvent();
    public AsyncEvent ApplyClicked { get; } = new AsyncEvent();

    public abstract RectTransform Panel { get; }
    public abstract IReadOnlyList<RectTransform> AnimatedElements { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);

    protected UniTask RaiseBackClicked()
    {
        return BackClicked.InvokeAsync();
    }

    protected UniTask RaiseApplyClicked()
    {
        return ApplyClicked.InvokeAsync();
    }
}
}