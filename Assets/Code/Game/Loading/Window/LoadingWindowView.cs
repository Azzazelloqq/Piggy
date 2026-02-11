using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.Loading.Window
{
public sealed class LoadingWindowView : LoadingWindowViewBase
{
    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private bool _disableGameObjectOnHide = true;

    [SerializeField]
    private Slider _progressBar;

    [SerializeField]
    private float _fadeDuration = 0.0f;

    public override void SetVisible(bool isVisible)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
        }

        if (_disableGameObjectOnHide)
        {
            gameObject.SetActive(isVisible);
        }
    }

    public override void SetProgress(float progress)
    {
        if (_progressBar != null)
        {
            _progressBar.value = Mathf.Clamp01(progress);
        }
    }

    public override async UniTask ShowAsync(CancellationToken token)
    {
        if (_canvasGroup == null || _fadeDuration <= 0f)
        {
            SetVisible(true);
            return;
        }

        if (_disableGameObjectOnHide)
        {
            gameObject.SetActive(true);
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        await FadeAsync(0f, 1f, token);
    }

    public override async UniTask HideAsync(CancellationToken token)
    {
        if (_canvasGroup == null || _fadeDuration <= 0f)
        {
            SetVisible(false);
            return;
        }

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        await FadeAsync(_canvasGroup.alpha, 0f, token);

        if (_disableGameObjectOnHide)
        {
            gameObject.SetActive(false);
        }
    }

    private async UniTask FadeAsync(float from, float to, CancellationToken token)
    {
        if (_fadeDuration <= 0f)
        {
            _canvasGroup.alpha = to;
            return;
        }

        var time = 0f;
        _canvasGroup.alpha = from;

        while (time < _fadeDuration)
        {
            token.ThrowIfCancellationRequested();
            time += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(time / _fadeDuration);
            _canvasGroup.alpha = Mathf.Lerp(from, to, t);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        _canvasGroup.alpha = to;
    }
}
}