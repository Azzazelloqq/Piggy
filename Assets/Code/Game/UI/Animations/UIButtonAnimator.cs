using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Code.UI.Animations
{
    [RequireComponent(typeof(Selectable))]
    public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scale Animation")]
        public float hoverScale = 1.05f;
        public float clickScale = 0.95f;
        public float duration = 0.2f;

        private Vector3 _originalScale;
        private Selectable _selectable;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _selectable = GetComponent<Selectable>();
        }

        private void OnDisable()
        {
            transform.DOKill();
            transform.localScale = _originalScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_selectable.interactable) return;
            transform.DOScale(_originalScale * hoverScale, duration).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_selectable.interactable) return;
            transform.DOScale(_originalScale, duration).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_selectable.interactable) return;
            transform.DOScale(_originalScale * clickScale, duration / 2f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_selectable.interactable) return;
            transform.DOScale(_originalScale * hoverScale, duration / 2f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }
}
