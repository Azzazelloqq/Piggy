using System.Collections.Generic;
using Code.Game.Async;
using MVP;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public abstract class GameProfilesViewBase
    : ViewMonoBehaviour<GameProfilesPresenterBase>
{
    public abstract AsyncEvent BackClicked { get; }

    public abstract RectTransform Panel { get; }
    public abstract IReadOnlyList<RectTransform> AnimatedElements { get; }
    public abstract GameProfilesDeleteConfirmViewBase DeleteConfirmView { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);
    public abstract IReadOnlyList<GameProfilesSlotViewBase> EnsureSlotViews(int count);
}
}
