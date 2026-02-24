using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public interface IMainMenuScreenTransitionAnimator
{
    MainMenuScreenTransitionDirection GetDefaultEnterDirection(MainMenuScreen screen);

    Vector2 GetHiddenPosition(
        RectTransform panel,
        Vector2 shownPosition,
        MainMenuScreenTransitionDirection direction,
        MainMenuViewBase.LayoutData layout);

    void ApplyPanelImmediate(
        MainMenuScreenTransitionView.PanelHandle panel,
        Vector2 position,
        bool show);

    void PreparePanelForShow(
        MainMenuScreenTransitionView.PanelHandle panel,
        Vector2 hiddenPosition);

    UniTask MovePanelAsync(
        MainMenuScreenTransitionView.PanelHandle panel,
        Vector2 position,
        bool show,
        MainMenuViewBase.LayoutData layout,
        CancellationToken token);
}
}
