using System;
using System.Collections.Generic;
using MVP;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuSettingsViewBase
    : ViewMonoBehaviour<MainMenuSettingsPresenterBase>
{
    public event Action BackClicked;
    public event Action ApplyClicked;

    public abstract RectTransform Panel { get; }
    public abstract IReadOnlyList<RectTransform> AnimatedElements { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);

    protected void RaiseBackClicked()
    {
        BackClicked?.Invoke();
    }

    protected void RaiseApplyClicked()
    {
        ApplyClicked?.Invoke();
    }
}
}