using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuView : MainMenuViewBase
{
    [Header("Layout")]
    [SerializeField]
    private LayoutData _layout = new LayoutData();

    [Header("Panel")]
    [SerializeField]
    private RectTransform _panel;

    [Header("Child Views")]
    [SerializeField]
    private MainMenuSettingsViewBase _settingsView;

    [SerializeField]
    private MainMenuExitConfirmViewBase _exitConfirmView;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private bool _disableGameObjectOnHide = true;

    [Header("Buttons")]
    [SerializeField]
    private Button _playButton;

    [SerializeField]
    private Button _settingsButton;

    [SerializeField]
    private Button _exitButton;

    public override RectTransform Panel => _panel;
    public override LayoutData Layout => _layout;

    internal override MainMenuSettingsViewBase SettingsView => _settingsView;
    internal override MainMenuExitConfirmViewBase ExitConfirmView => _exitConfirmView;

    public override void SetVisible(bool isVisible)
    {
        _canvasGroup.alpha = isVisible ? 1f : 0f;
        _canvasGroup.interactable = isVisible;
        _canvasGroup.blocksRaycasts = isVisible;

        if (_disableGameObjectOnHide && _panel.gameObject != gameObject)
        {
            _panel.gameObject.SetActive(isVisible);
        }
    }

    public override void SetInteractable(bool isInteractable)
    {
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.blocksRaycasts = isInteractable;
    }

    protected override void OnInitialize()
    {
        SubscribeOnEvents();
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeOnEvents();
        
        return default;
    }

    protected override void OnDispose()
    {
        UnsubscribeOnEvents();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeOnEvents();

        return default;
    }

    private void SubscribeOnEvents()
    {
        _playButton.onClick.AddListener(HandlePlayClicked);
        _settingsButton.onClick.AddListener(HandleSettingsClicked);
        _exitButton.onClick.AddListener(HandleExitClicked);
    }

    private void UnsubscribeOnEvents()
    {
        _playButton.onClick.RemoveListener(HandlePlayClicked);
        _settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        _exitButton.onClick.RemoveListener(HandleExitClicked);
    }

    private void HandlePlayClicked()
    {
        RaisePlayClicked();
    }

    private void HandleSettingsClicked()
    {
        RaiseSettingsClicked();
    }

    private void HandleExitClicked()
    {
        RaiseExitClicked();
    }
}
}