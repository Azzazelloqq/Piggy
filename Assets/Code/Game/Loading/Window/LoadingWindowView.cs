using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

    [SerializeField]
    private Ease _fadeInEase = Ease.OutCubic;

    [SerializeField]
    private Ease _fadeOutEase = Ease.InCubic;

    [SerializeField]
    private bool _useUnscaledTime = true;

    private Tween _fadeTween;

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

    public override void SetProgress(float progress)
    {
        _progressBar.value = Mathf.Clamp01(progress);
    }

    public override async UniTask ShowAsync(CancellationToken token)
    {
        if (_fadeDuration <= 0f)
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

        await FadeAsync(0f, 1f, _fadeInEase, token);
    }

    public override async UniTask HideAsync(CancellationToken token)
    {
        if (_fadeDuration <= 0f)
        {
            SetVisible(false);
            return;
        }

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        await FadeAsync(_canvasGroup.alpha, 0f, _fadeOutEase, token);

        if (_disableGameObjectOnHide)
        {
            gameObject.SetActive(false);
        }
    }

    protected override void OnInitialize()
    {
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        return default;
    }

    protected override void OnDispose()
    {
        KillFadeTween();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        KillFadeTween();
        
        return default;
    }

    private async UniTask FadeAsync(float from, float to, Ease ease, CancellationToken token)
    {
        if (_fadeDuration <= 0f)
        {
            _canvasGroup.alpha = to;
            return;
        }

        KillFadeTween();
        _canvasGroup.alpha = from;

        _fadeTween = _canvasGroup.DOFade(to, _fadeDuration)
            .SetEase(ease)
            .SetUpdate(_useUnscaledTime);

        using (token.Register(() => _fadeTween.Kill(false)))
        {
            await _fadeTween.AsyncWaitForCompletion();
        }

        if (token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();
        }

        _fadeTween = null;
    }

    private void KillFadeTween()
    {
        if (_fadeTween == null)
        {
            return;
        }

        _fadeTween.Kill(false);
        _fadeTween = null;
    }
}
}