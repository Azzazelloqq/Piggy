using UnityEngine;
using DG.Tweening;

namespace Code.UI.Animations
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIWindowAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float duration = 0.3f;
        public float startScale = 0.9f;
        public Ease easeType = Ease.OutBack;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            // Reset to start state
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * startScale;

            // Animate
            _canvasGroup.DOFade(1f, duration).SetUpdate(true);
            transform.DOScale(Vector3.one, duration).SetEase(easeType).SetUpdate(true);
        }

        private void OnDisable()
        {
            transform.DOKill();
            _canvasGroup.DOKill();
        }
        
        public void Hide(System.Action onComplete = null)
        {
            _canvasGroup.DOFade(0f, duration).SetUpdate(true);
            transform.DOScale(Vector3.one * startScale, duration).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
