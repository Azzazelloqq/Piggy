using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuExitConfirmView : MainMenuExitConfirmViewBase
{
    [Header("Panel")]
    [SerializeField]
    private RectTransform _panel;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private bool _disableGameObjectOnHide = true;

    [Header("Buttons")]
    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private Button _cancelButton;

    public override RectTransform Panel => _panel;

    public override void SetVisible(bool isVisible)
    {
        _canvasGroup.alpha = isVisible ? 1f : 0f;
        _canvasGroup.interactable = isVisible;
        _canvasGroup.blocksRaycasts = isVisible;

        if (_disableGameObjectOnHide)
        {
            gameObject.SetActive(isVisible);
        }
    }

    public override void SetInteractable(bool isInteractable)
    {
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.blocksRaycasts = isInteractable;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        SubscribeOnEvents();
    }

    protected override async ValueTask OnInitializeAsync(CancellationToken token)
    {
        await base.OnInitializeAsync(token);
        SubscribeOnEvents();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        UnsubscribeOnEvents();
    }

    protected override async ValueTask OnDisposeAsync(CancellationToken token)
    {
        await base.OnDisposeAsync(token);
        UnsubscribeOnEvents();
    }

    private void SubscribeOnEvents()
    {
        _confirmButton.onClick.AddListener(HandleConfirmClicked);

        _cancelButton.onClick.AddListener(HandleCancelClicked);
    }

    private void UnsubscribeOnEvents()
    {
        _confirmButton.onClick.RemoveListener(HandleConfirmClicked);

        _cancelButton.onClick.RemoveListener(HandleCancelClicked);
    }

    private void HandleConfirmClicked()
    {
        RaiseConfirmClicked();
    }

    private void HandleCancelClicked()
    {
        RaiseCancelClicked();
    }
}
}