using System;
using System.Collections.Generic;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;
using DG.Tweening;
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
        [HideInInspector]
        private float _showOvershoot = 1.2f;

        [SerializeField]
        private float _offscreenPadding = 60f;

        [SerializeField]
        private bool _useUnscaledTime = true;

        [Header("Panel Motion")]
        [SerializeField]
        private Ease _panelShowEase = Ease.OutBack;

        [SerializeField]
        private Ease _panelHideEase = Ease.InCubic;

        [SerializeField]
        [Min(0)]
        private int _panelShowSteps = 0;

        [SerializeField]
        [Min(0)]
        private int _panelHideSteps = 0;

        [SerializeField]
        private float _panelShowOvershoot = 1.2f;

        [SerializeField]
        private float _panelHideOvershoot = 0f;

        [Header("Content Motion")]
        [SerializeField]
        private Ease _contentShowEase = Ease.OutBack;

        [SerializeField]
        private Ease _contentHideEase = Ease.InCubic;

        [SerializeField]
        [Min(0)]
        private int _contentShowSteps = 0;

        [SerializeField]
        [Min(0)]
        private int _contentHideSteps = 0;

        [SerializeField]
        private float _contentShowOvershoot = 1.2f;

        [SerializeField]
        private float _contentHideOvershoot = 0f;

        [Header("Shown Positions")]
        [SerializeField]
        private Vector2 _menuShown;

        [SerializeField]
        private Vector2 _settingsShown;

        [SerializeField]
        private Vector2 _exitShown;

        public float TransitionDuration => _transitionDuration;
        public float ShowOvershoot => _showOvershoot;
        public float OffscreenPadding => _offscreenPadding;
        public bool UseUnscaledTime => _useUnscaledTime;
        public Ease PanelShowEase => _panelShowEase;
        public Ease PanelHideEase => _panelHideEase;
        public int PanelShowSteps => _panelShowSteps;
        public int PanelHideSteps => _panelHideSteps;
        public float PanelShowOvershoot => _panelShowOvershoot;
        public float PanelHideOvershoot => _panelHideOvershoot;
        public Ease ContentShowEase => _contentShowEase;
        public Ease ContentHideEase => _contentHideEase;
        public int ContentShowSteps => _contentShowSteps;
        public int ContentHideSteps => _contentHideSteps;
        public float ContentShowOvershoot => _contentShowOvershoot;
        public float ContentHideOvershoot => _contentHideOvershoot;
        public Vector2 MenuShown => _menuShown;
        public Vector2 SettingsShown => _settingsShown;
        public Vector2 ExitShown => _exitShown;
    }

    public AsyncEvent PlayClicked { get; } = new AsyncEvent();
    public AsyncEvent SettingsClicked { get; } = new AsyncEvent();
    public AsyncEvent ExitClicked { get; } = new AsyncEvent();

    public abstract RectTransform Panel { get; }
    public abstract LayoutData Layout { get; }
    public abstract IReadOnlyList<RectTransform> AnimatedElements { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);
    internal abstract MainMenuSettingsViewBase SettingsView { get; }
    internal abstract MainMenuExitConfirmViewBase ExitConfirmView { get; }

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