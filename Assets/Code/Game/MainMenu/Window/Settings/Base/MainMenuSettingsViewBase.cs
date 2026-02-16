using System;
using System.Threading;
using System.Threading.Tasks;
using MVP;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuSettingsViewBase
    : ViewMonoBehaviour<MainMenuSettingsPresenterBase>,
        IMainMenuPanelView
{
    public event Action BackClicked;
    public event Action ApplyClicked;

    public abstract RectTransform Panel { get; }
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