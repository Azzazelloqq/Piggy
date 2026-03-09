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
    private static readonly Color TextColor = new(0.22f, 0.17f, 0.12f, 1f);
    private static readonly Color PanelColor = new(0f, 0f, 0f, 0.2f);
    private static readonly Color ButtonColor = new(0.9f, 0.85f, 0.75f, 1f);
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
    private TMP_Text _avatarLabel;

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
    private CancellationTokenSource _subscriptionsCts;
    private UniTask _subscriptionsTask;
    private CancellationTokenSource _avatarSlideCts;
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

    public override void SetAvatar(Sprite portrait, string label, AvatarSlideDirection direction)
    {
        EnsureBuilt();
        _avatarImage.sprite = portrait;
        _avatarImage.enabled = portrait != null;
        _avatarLabel.text = label ?? string.Empty;
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

        if (!HasSerializedLayout())
        {
            throw new InvalidOperationException("CharacterCreationView: serialized layout is incomplete.");
        }

        if (!_avatarRestPositionCached)
        {
            _avatarRestPosition = _avatarImage.rectTransform.anchoredPosition;
            _avatarRestPositionCached = true;
        }

        _built = true;
    }

    private void StartAvatarSlide(AvatarSlideDirection direction)
    {
        StopAvatarSlide();
        var rect = _avatarImage.rectTransform;
        var parent = rect.parent as RectTransform;
        if (parent == null)
        {
            rect.anchoredPosition = _avatarRestPosition;
            return;
        }

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
        _avatarSlideCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy());
        _ = AnimateAvatarSlideAsync(rect, startX, _avatarRestPosition.x, AvatarSlideDuration, _avatarSlideCts.Token);
    }

    private void StopAvatarSlide()
    {
        if (_avatarSlideCts == null)
        {
            return;
        }

        _avatarSlideCts.Cancel();
        _avatarSlideCts.Dispose();
        _avatarSlideCts = null;
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

    private void BuildAvatarPanel(RectTransform parent)
    {
        var panel = CreateRect("AvatarPanel", parent);
        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        var title = CreateText("AvatarTitle", panel, "Вид персонажа", 28, TextAlignmentOptions.Center);
        var titleLocalized = title.gameObject.AddComponent<LocalizedTMPText>();
        titleLocalized.Key = "ui.character_creation.avatar";
        titleLocalized.Fallback = "Вид персонажа";

        var portrait = CreateRect("AvatarImage", panel);
        portrait.sizeDelta = new Vector2(360f, 360f);
        _avatarImage = portrait.gameObject.AddComponent<Image>();
        _avatarImage.color = new Color(1f, 1f, 1f, 0.9f);
        _avatarImage.preserveAspect = true;

        _avatarLabel = CreateText("AvatarLabel", panel, string.Empty, 22, TextAlignmentOptions.Center);

        var buttons = CreateRect("AvatarButtons", panel);
        var buttonsLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
        buttonsLayout.spacing = 10f;
        buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonsLayout.childControlHeight = true;
        buttonsLayout.childForceExpandHeight = false;
        buttonsLayout.childControlWidth = true;
        buttonsLayout.childForceExpandWidth = false;

        _avatarPrevButton = CreateButton("AvatarPrevButton", buttons, "<", "ui.character_creation.avatar_prev");
        _avatarNextButton = CreateButton("AvatarNextButton", buttons, ">", "ui.character_creation.avatar_next");
        var prevSize = _avatarPrevButton.gameObject.AddComponent<LayoutElement>();
        prevSize.preferredWidth = 60f;
        prevSize.preferredHeight = 40f;
        var nextSize = _avatarNextButton.gameObject.AddComponent<LayoutElement>();
        nextSize.preferredWidth = 60f;
        nextSize.preferredHeight = 40f;
    }

    private void BuildNamePanel(RectTransform parent)
    {
        var panel = CreateRect("NamePanel", parent);
        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        var label = CreateText("NameLabel", panel, "Имя", 28, TextAlignmentOptions.Center);
        var localized = label.gameObject.AddComponent<LocalizedTMPText>();
        localized.Key = "ui.character_creation.name";
        localized.Fallback = "Имя";

        _nameInput = CreateInputField(panel, "Введите имя");
    }

    private void BuildStatsPanel(RectTransform parent)
    {
        var panel = CreateRect("StatsPanel", parent);
        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        var header = CreateRect("StatsHeader", panel);
        var headerLayout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
        headerLayout.spacing = 12f;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandHeight = false;
        headerLayout.childControlWidth = true;
        headerLayout.childForceExpandWidth = false;

        var title = CreateText("StatsTitle", header, "Характеристики", 28, TextAlignmentOptions.Left);
        var localized = title.gameObject.AddComponent<LocalizedTMPText>();
        localized.Key = "ui.character_creation.stats";
        localized.Fallback = "Характеристики";

        _pointsText = CreateText("PointsText", header, "Очки: 0/0", 22, TextAlignmentOptions.Right);
        var pointsSize = _pointsText.gameObject.AddComponent<LayoutElement>();
        pointsSize.preferredWidth = 160f;

        _statsRoot = CreateRect("StatsList", panel);
        var listLayout = _statsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 8f;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandHeight = false;
        listLayout.childControlWidth = true;
        listLayout.childForceExpandWidth = true;
    }

    private void BuildTraitsPanel(RectTransform parent)
    {
        var panel = CreateRect("TraitsPanel", parent);
        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        var header = CreateRect("TraitsHeader", panel);
        var headerLayout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
        headerLayout.spacing = 12f;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandHeight = false;
        headerLayout.childControlWidth = true;
        headerLayout.childForceExpandWidth = false;

        var title = CreateText("TraitsTitle", header, "Черты", 28, TextAlignmentOptions.Left);
        var localized = title.gameObject.AddComponent<LocalizedTMPText>();
        localized.Key = "ui.character_creation.traits";
        localized.Fallback = "Черты";

        _traitsText = CreateText("TraitsCount", header, "Выбрано: 0/0", 22, TextAlignmentOptions.Right);
        var traitsSize = _traitsText.gameObject.AddComponent<LayoutElement>();
        traitsSize.preferredWidth = 180f;

        _traitsRoot = CreateRect("TraitsList", panel);
        var listLayout = _traitsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 6f;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandHeight = false;
        listLayout.childControlWidth = true;
        listLayout.childForceExpandWidth = true;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private TMP_Text CreateText(
        string name,
        Transform parent,
        string text,
        int fontSize,
        TextAlignmentOptions alignment)
    {
        var rect = CreateRect(name, parent);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text ?? string.Empty;
        label.fontSize = fontSize;
        label.color = TextColor;
        label.alignment = alignment;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        return label;
    }

    private Button CreateButton(string name, Transform parent, string label, string localizationKey)
    {
        var rect = CreateRect(name, parent);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = ButtonColor;
        var button = rect.gameObject.AddComponent<Button>();

        var text = CreateText("Label", rect, label, 24, TextAlignmentOptions.Center);
        var localized = text.gameObject.AddComponent<LocalizedTMPText>();
        localized.Key = localizationKey;
        localized.Fallback = label;

        return button;
    }

    private TMP_InputField CreateInputField(Transform parent, string placeholderText)
    {
        var root = CreateRect("NameInput", parent);
        var background = root.gameObject.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.85f);

        var input = root.gameObject.AddComponent<TMP_InputField>();

        var viewport = CreateRect("Viewport", root);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.sizeDelta = new Vector2(-10f, -10f);
        var mask = viewport.gameObject.AddComponent<RectMask2D>();
        mask.enabled = true;

        var text = CreateText("Text", viewport, string.Empty, 24, TextAlignmentOptions.Left);
        text.enableWordWrapping = false;
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-10f, 0f);

        var placeholder = CreateText("Placeholder", viewport, placeholderText, 24, TextAlignmentOptions.Left);
        placeholder.color = new Color(TextColor.r, TextColor.g, TextColor.b, 0.45f);
        var placeholderRect = placeholder.rectTransform;
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = new Vector2(-10f, 0f);

        input.textComponent = text;
        input.placeholder = placeholder;
        input.textViewport = viewport;
        input.pointSize = 24;
        input.text = string.Empty;

        var inputSize = root.gameObject.AddComponent<LayoutElement>();
        inputSize.preferredHeight = 44f;

        return input;
    }

    private CharacterStatRowViewBase CreateStatRow(Transform parent)
    {
        return Instantiate(_statRowPrefab, parent, false);
    }

    private CharacterTraitRowViewBase CreateTraitRow(Transform parent)
    {
        return Instantiate(_traitRowPrefab, parent, false);
    }

    private bool HasSerializedLayout()
    {
        return _panel != null
               && _canvasGroup != null
               && _animatedElements != null
               && _nameInput != null
               && _pointsText != null
               && _traitsText != null
               && _avatarLabel != null
               && _avatarImage != null
               && _createButton != null
               && _backButton != null
               && _closeButton != null
               && _avatarPrevButton != null
               && _avatarNextButton != null
               && _statsRoot != null
               && _traitsRoot != null
               && _statRowPrefab != null
               && _traitRowPrefab != null;
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
        StopSubscriptionsImmediate();

        _subscriptionsCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            this.GetCancellationTokenOnDestroy());
        _subscriptionsTask = RunButtonSubscriptionsAsync(_subscriptionsCts.Token);

        _nameInput.onValueChanged.AddListener(HandleNameChanged);
    }

    private void StopSubscriptionsImmediate()
    {
        _nameInput.onValueChanged.RemoveListener(HandleNameChanged);

        if (_subscriptionsCts == null)
        {
            return;
        }

        _subscriptionsCts.Cancel();
        _subscriptionsCts.Dispose();
        _subscriptionsCts = null;
        _subscriptionsTask = default;
    }

    private async UniTask StopSubscriptionsAsync()
    {
        _nameInput.onValueChanged.RemoveListener(HandleNameChanged);

        if (_subscriptionsCts == null)
        {
            return;
        }

        _subscriptionsCts.Cancel();
        _subscriptionsCts.Dispose();
        _subscriptionsCts = null;

        await _subscriptionsTask;
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
