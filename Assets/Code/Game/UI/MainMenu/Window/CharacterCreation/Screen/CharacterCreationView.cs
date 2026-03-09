using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using LocalizedDomain.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.MainMenu.Window
{
public sealed class CharacterCreationView : CharacterCreationViewBase
{
    private const float AvatarSlideDuration = 0.2f;

    [SerializeField]
    private RectTransform _panel;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private bool _disableGameObjectOnHide = true;

    [SerializeField]
    private List<RectTransform> _animatedElements = new();

    [SerializeField]
    private TMP_InputField _nameInput;

    [SerializeField]
    private RectTransform _nameInputRoot;

    [SerializeField]
    private TMP_Text _pointsText;

    [SerializeField]
    private TMP_Text _traitsText;

    [SerializeField]
    private LocalizedTMPText _avatarLabelLocalized;

    [SerializeField]
    private Image _avatarImage;

    [SerializeField]
    private Button _createButton;

    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private Button _closeButton;

    [SerializeField]
    private Button _avatarPrevButton;

    [SerializeField]
    private Button _avatarNextButton;

    [SerializeField]
    private RectTransform _statsRoot;

    [SerializeField]
    private RectTransform _traitsRoot;

    [SerializeField]
    private CharacterStatRowView _statRowPrefab;

    [SerializeField]
    private CharacterTraitRowView _traitRowPrefab;

    private readonly List<CharacterStatRowViewBase> _statRows = new();
    private readonly List<CharacterTraitRowViewBase> _traitRows = new();

    private bool _built;
    private CancellationTokenSource _subscriptionsCts = new();
    private UniTask _subscriptionsTask;
    private CancellationTokenSource _avatarSlideCts = new();
    private Vector2 _avatarRestPosition;
    private bool _avatarRestPositionCached;

    public override RectTransform Panel => _panel;
    public override IReadOnlyList<RectTransform> AnimatedElements => _animatedElements;

    public override void SetVisible(bool isVisible)
    {
        EnsureBuilt();

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
        EnsureBuilt();
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.blocksRaycasts = isInteractable;
    }

    public override void SetCreateInteractable(bool isInteractable)
    {
        EnsureBuilt();
        _createButton.interactable = isInteractable;
    }

    public override void SetName(string name)
    {
        EnsureBuilt();
        _nameInput.SetTextWithoutNotify(name ?? string.Empty);
    }

    public override void SetPointsText(string text)
    {
        EnsureBuilt();
        _pointsText.text = text ?? string.Empty;
    }

    public override void SetTraitsText(string text)
    {
        EnsureBuilt();
        _traitsText.text = text ?? string.Empty;
    }

    public override void SetAvatar(
        Sprite portrait,
        string localizationKey,
        string fallbackLabel,
        AvatarSlideDirection direction)
    {
        EnsureBuilt();
        _avatarImage.sprite = portrait;
        _avatarImage.enabled = true;
        SetAvatarLabel(localizationKey, fallbackLabel);
        StartAvatarSlide(direction);
    }

    public override IReadOnlyList<CharacterStatRowViewBase> EnsureStatRows(int count)
    {
        EnsureBuilt();
        EnsureRowCount(count, _statRows, _statsRoot, CreateStatRow);
        return _statRows;
    }

    public override IReadOnlyList<CharacterTraitRowViewBase> EnsureTraitRows(int count)
    {
        EnsureBuilt();
        EnsureRowCount(count, _traitRows, _traitsRoot, CreateTraitRow);
        return _traitRows;
    }

    protected override void OnInitialize()
    {
        EnsureBuilt();
        SubscribeOnEvents(disposeToken);
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        EnsureBuilt();
        SubscribeOnEvents(token);
        return default;
    }

    protected override void OnDispose()
    {
        StopSubscriptionsImmediate();
        StopAvatarSlide();
    }

    protected override async ValueTask OnDisposeAsync(CancellationToken token)
    {
        await StopSubscriptionsAsync();
        StopAvatarSlide();
    }

    private void EnsureBuilt()
    {
        if (_built)
        {
            return;
        }

        if (!_avatarRestPositionCached)
        {
            _avatarRestPosition = _avatarImage.rectTransform.anchoredPosition;
            _avatarRestPositionCached = true;
        }

        _built = true;
    }

    private void SetAvatarLabel(string localizationKey, string fallbackLabel)
    {
        _avatarLabelLocalized.Key = localizationKey;
        _avatarLabelLocalized.Fallback = fallbackLabel ?? string.Empty;
    }

    private void StartAvatarSlide(AvatarSlideDirection direction)
    {
        _avatarSlideCts.Cancel();
        _avatarSlideCts.Dispose();
        _avatarSlideCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy());
        var rect = _avatarImage.rectTransform;
        var parent = (RectTransform)rect.parent;

        if (direction == AvatarSlideDirection.None || !gameObject.activeInHierarchy)
        {
            rect.anchoredPosition = _avatarRestPosition;
            return;
        }

        var width = parent.rect.width;
        if (width <= 0f)
        {
            rect.anchoredPosition = _avatarRestPosition;
            return;
        }

        var startX = direction == AvatarSlideDirection.FromLeft ? -width : width;
        rect.anchoredPosition = new Vector2(startX, _avatarRestPosition.y);
        _ = AnimateAvatarSlideAsync(rect, startX, _avatarRestPosition.x, AvatarSlideDuration, _avatarSlideCts.Token);
    }

    private void StopAvatarSlide()
    {
        _avatarSlideCts.Cancel();
        _avatarSlideCts.Dispose();
        _avatarSlideCts = new CancellationTokenSource();
    }

    private static async UniTask AnimateAvatarSlideAsync(
        RectTransform rect,
        float startX,
        float endX,
        float duration,
        CancellationToken token)
    {
        var elapsed = 0f;
        while (elapsed < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var pos = rect.anchoredPosition;
            pos.x = Mathf.Lerp(startX, endX, t);
            rect.anchoredPosition = pos;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        var finalPos = rect.anchoredPosition;
        finalPos.x = endX;
        rect.anchoredPosition = finalPos;
    }

    private CharacterStatRowViewBase CreateStatRow(Transform parent)
    {
        return Instantiate(_statRowPrefab, parent, false);
    }

    private CharacterTraitRowViewBase CreateTraitRow(Transform parent)
    {
        return Instantiate(_traitRowPrefab, parent, false);
    }

    private static void EnsureRowCount<T>(
        int count,
        List<T> rows,
        Transform root,
        Func<Transform, T> createRow)
        where T : Component
    {
        while (rows.Count < count)
        {
            var row = createRow(root);
            rows.Add(row);
        }

        while (rows.Count > count)
        {
            var lastIndex = rows.Count - 1;
            var row = rows[lastIndex];
            Destroy(row.gameObject);
            rows.RemoveAt(lastIndex);
        }
    }

    private void SubscribeOnEvents(CancellationToken token)
    {
        _nameInput.onValueChanged.RemoveListener(HandleNameChanged);
        _subscriptionsCts.Cancel();
        _subscriptionsCts.Dispose();
        _subscriptionsCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            this.GetCancellationTokenOnDestroy());
        _subscriptionsTask = RunButtonSubscriptionsAsync(_subscriptionsCts.Token);

        _nameInput.onValueChanged.AddListener(HandleNameChanged);
    }

    private void StopSubscriptionsImmediate()
    {
        _nameInput.onValueChanged.RemoveListener(HandleNameChanged);
        _subscriptionsCts.Cancel();
        _subscriptionsCts.Dispose();
        _subscriptionsCts = new CancellationTokenSource();
        _subscriptionsTask = default;
    }

    private async UniTask StopSubscriptionsAsync()
    {
        _nameInput.onValueChanged.RemoveListener(HandleNameChanged);
        var task = _subscriptionsTask;
        _subscriptionsCts.Cancel();
        _subscriptionsCts.Dispose();
        _subscriptionsCts = new CancellationTokenSource();

        await task;
        _subscriptionsTask = default;
    }

    private async UniTask RunButtonSubscriptionsAsync(CancellationToken token)
    {
        var tasks = new List<UniTask>(5)
        {
            WaitForClicksAsync(_createButton, RaiseCreateClicked, token),
            WaitForClicksAsync(_backButton, RaiseBackClicked, token),
            WaitForClicksAsync(_closeButton, RaiseBackClicked, token),
            WaitForClicksAsync(_avatarPrevButton, RaiseAvatarPrevClicked, token),
            WaitForClicksAsync(_avatarNextButton, RaiseAvatarNextClicked, token)
        };

        await UniTask.WhenAll(tasks);
    }

    private void HandleNameChanged(string value)
    {
        RaiseNameChanged(value).Forget();
    }

    private static async UniTask WaitForClicksAsync(
        Button button,
        Func<UniTask> onClick,
        CancellationToken token)
    {
        try
        {
            await foreach (var _ in button.OnClickAsAsyncEnumerable(token))
            {
                try
                {
                    await onClick();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
}
