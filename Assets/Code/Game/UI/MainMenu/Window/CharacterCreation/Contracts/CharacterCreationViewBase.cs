using System.Collections.Generic;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public abstract class CharacterCreationViewBase
    : ViewMonoBehaviour<CharacterCreationPresenterBase>
{
    public AsyncEvent BackClicked { get; } = new();
    public AsyncEvent CreateClicked { get; } = new();
    public AsyncEvent<string> NameChanged { get; } = new();
    public AsyncEvent AvatarPrevClicked { get; } = new();
    public AsyncEvent AvatarNextClicked { get; } = new();

    public abstract RectTransform Panel { get; }
    public abstract IReadOnlyList<RectTransform> AnimatedElements { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);
    public abstract void SetCreateInteractable(bool isInteractable);
    public abstract void SetName(string name);
    public abstract void SetPointsText(string text);
    public abstract void SetTraitsText(string text);
    public abstract void SetAvatar(
        Sprite portrait,
        string localizationKey,
        string fallbackLabel,
        AvatarSlideDirection direction);
    public abstract IReadOnlyList<CharacterStatRowViewBase> EnsureStatRows(int count);
    public abstract IReadOnlyList<CharacterTraitRowViewBase> EnsureTraitRows(int count);

    protected UniTask RaiseBackClicked()
    {
        return BackClicked.InvokeAsync();
    }

    protected UniTask RaiseCreateClicked()
    {
        return CreateClicked.InvokeAsync();
    }

    protected UniTask RaiseAvatarPrevClicked()
    {
        return AvatarPrevClicked.InvokeAsync();
    }

    protected UniTask RaiseAvatarNextClicked()
    {
        return AvatarNextClicked.InvokeAsync();
    }

    protected UniTask RaiseNameChanged(string name)
    {
        return NameChanged.InvokeAsync(name);
    }
}
}
