using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
    public sealed class MainMenuSettingsView : MainMenuSettingsViewBase
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
        private Button _applyButton;

        [SerializeField]
        private Button _backButton;

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
            _applyButton.onClick.AddListener(HandleApplyClicked);
            _backButton.onClick.AddListener(HandleBackClicked);
        }

        private void UnsubscribeOnEvents()
        {
            _applyButton.onClick.RemoveListener(HandleApplyClicked);
            _backButton.onClick.RemoveListener(HandleBackClicked);
        }

        private void HandleApplyClicked()
        {
            RaiseApplyClicked();
        }

        private void HandleBackClicked()
        {
            RaiseBackClicked();
        }

    }
}
