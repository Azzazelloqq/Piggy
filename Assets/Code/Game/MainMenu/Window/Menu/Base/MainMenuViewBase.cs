using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using MVP;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public abstract class MainMenuViewBase
    : ViewMonoBehaviour<MainMenuPresenterBase>,
        IMainMenuPanelView
{
    [Serializable]
    public sealed class LayoutData
    {
        [Header("Transition")]
        [SerializeField]
        private float _transitionDuration = 0.35f;

        [SerializeField]
        private Ease _showEase = Ease.OutCubic;

        [SerializeField]
        private Ease _hideEase = Ease.InCubic;

        [SerializeField]
        private bool _useUnscaledTime = true;

        [Header("Menu Positions")]
        [SerializeField]
        private Vector2 _menuShown;

        [SerializeField]
        private Vector2 _menuHiddenRight;

        [SerializeField]
        private Vector2 _menuHiddenUp;

        [Header("Settings Positions")]
        [SerializeField]
        private Vector2 _settingsShown;

        [SerializeField]
        private Vector2 _settingsHiddenLeft;

        [Header("Exit Confirm Positions")]
        [SerializeField]
        private Vector2 _exitShown;

        [SerializeField]
        private Vector2 _exitHiddenBottom;

        public float TransitionDuration => _transitionDuration;
        public Ease ShowEase => _showEase;
        public Ease HideEase => _hideEase;
        public bool UseUnscaledTime => _useUnscaledTime;

        public Vector2 GetMenuPosition(MainMenuPosition position)
        {
            return position switch
            {
                MainMenuPosition.Shown => _menuShown,
                MainMenuPosition.HiddenRight => _menuHiddenRight,
                MainMenuPosition.HiddenUp => _menuHiddenUp,
                _ => _menuShown
            };
        }

        public Vector2 GetSettingsPosition(MainMenuSettingsPosition position)
        {
            return position switch
            {
                MainMenuSettingsPosition.Shown => _settingsShown,
                MainMenuSettingsPosition.HiddenLeft => _settingsHiddenLeft,
                _ => _settingsShown
            };
        }

        public Vector2 GetExitPosition(MainMenuExitConfirmPosition position)
        {
            return position switch
            {
                MainMenuExitConfirmPosition.Shown => _exitShown,
                MainMenuExitConfirmPosition.HiddenBottom => _exitHiddenBottom,
                _ => _exitShown
            };
        }

        public void SetMenuPosition(MainMenuPosition position, Vector2 value)
        {
            switch (position)
            {
                case MainMenuPosition.Shown:
                    _menuShown = value;
                    break;
                case MainMenuPosition.HiddenRight:
                    _menuHiddenRight = value;
                    break;
                case MainMenuPosition.HiddenUp:
                    _menuHiddenUp = value;
                    break;
                default:
                    _menuShown = value;
                    break;
            }
        }

        public void SetSettingsPosition(MainMenuSettingsPosition position, Vector2 value)
        {
            switch (position)
            {
                case MainMenuSettingsPosition.Shown:
                    _settingsShown = value;
                    break;
                case MainMenuSettingsPosition.HiddenLeft:
                    _settingsHiddenLeft = value;
                    break;
                default:
                    _settingsShown = value;
                    break;
            }
        }

        public void SetExitPosition(MainMenuExitConfirmPosition position, Vector2 value)
        {
            switch (position)
            {
                case MainMenuExitConfirmPosition.Shown:
                    _exitShown = value;
                    break;
                case MainMenuExitConfirmPosition.HiddenBottom:
                    _exitHiddenBottom = value;
                    break;
                default:
                    _exitShown = value;
                    break;
            }
        }
    }

    public event Action PlayClicked;
    public event Action SettingsClicked;
    public event Action ExitClicked;

    public abstract RectTransform Panel { get; }
    public abstract LayoutData Layout { get; }
    public abstract void SetVisible(bool isVisible);
    public abstract void SetInteractable(bool isInteractable);
    internal abstract MainMenuSettingsViewBase SettingsView { get; }
    internal abstract MainMenuExitConfirmViewBase ExitConfirmView { get; }

    protected void RaisePlayClicked()
    {
        PlayClicked?.Invoke();
    }

    protected void RaiseSettingsClicked()
    {
        SettingsClicked?.Invoke();
    }

    protected void RaiseExitClicked()
    {
        ExitClicked?.Invoke();
    }
}
}