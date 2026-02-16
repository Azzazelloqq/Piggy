using System;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuSettingsModelBase : Model
{
    public event Action<bool> VisibilityChanged;
    public event Action BackRequested;
    public event Action ApplyRequested;

    public abstract bool IsVisible { get; }

    public abstract void Show();
    public abstract void Hide();
    public abstract void RequestBack();
    public abstract void RequestApply();

    protected void NotifyVisibilityChanged(bool isVisible)
    {
        VisibilityChanged?.Invoke(isVisible);
    }

    protected void NotifyBackRequested()
    {
        BackRequested?.Invoke();
    }

    protected void NotifyApplyRequested()
    {
        ApplyRequested?.Invoke();
    }
}
}