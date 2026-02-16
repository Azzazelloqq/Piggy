using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public interface IMainMenuPanelView
{
    RectTransform Panel { get; }
    void SetVisible(bool isVisible);
    void SetInteractable(bool isInteractable);
}
}