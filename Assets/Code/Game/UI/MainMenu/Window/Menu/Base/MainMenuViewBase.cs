using System;
using System.Collections.Generic;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuViewBase
    : ViewMonoBehaviour<MainMenuPresenterBase>
{
    [Serializable]
    public sealed class LayoutData
    {
        [Header("Transition")]
        [SerializeField]
        private float _transitionDuration = 0.35f;

        [SerializeField]
        private float _showOvershoot = 1.2f;

        [SerializeField]
        private float _offscreenPadding = 60f;

        [SerializeField]
        private bool _useUnscaledTime = true;

        [Header("Shown Positions")]
        [SerializeField]
        private Vector2 _menuShown;

        [SerializeField]
        private Vector2 _settingsShown;

        [SerializeField]
        private Vector2 _exitShown;

        [SerializeField]
        private Vector2 _savesShown;

        [SerializeField]
        private Vector2 _characterCreationShown;

        public float TransitionDuration => _transitionDuration;
        public float ShowOvershoot => _showOvershoot;
        public float OffscreenPadding => _offscreenPadding;
        public bool UseUnscaledTime => _useUnscaledTime;
        public Vector2 MenuShown => _menuShown;
        public Vector2 SettingsShown => _settingsShown;
        public Vector2 ExitShown => _exitShown;
        public Vector2 SavesShown => _savesShown;
        public Vector2 CharacterCreationShown => _characterCreationShown;
    }

    public AsyncEvent PlayClicked { get; } = new AsyncEvent();
    public AsyncEvent SettingsClicked { get; } = new AsyncEvent();
    public AsyncEvent ExitClicked { get; } = new AsyncEvent();

    public abstract RectTransform Panel { get; }
    public abstract LayoutData Layout { get; }
    public abstract IReadOnlyList<RectTransform> AnimatedElements { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);
    internal abstract SettingsPopupViewBase SettingsPopupView { get; }
    internal abstract ExitConfirmPopupViewBase ExitConfirmPopupView { get; }
    internal abstract GameProfilesViewBase SavesView { get; }
    internal abstract CharacterCreationViewBase CharacterCreationView { get; }

    protected UniTask RaisePlayClicked()
    {
        return PlayClicked.InvokeAsync();
    }

    protected UniTask RaiseSettingsClicked()
    {
        return SettingsClicked.InvokeAsync();
    }

    protected UniTask RaiseExitClicked()
    {
        return ExitClicked.InvokeAsync();
    }
}
}