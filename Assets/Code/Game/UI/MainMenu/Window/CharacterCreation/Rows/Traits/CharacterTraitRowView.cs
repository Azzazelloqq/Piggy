using System;
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
public sealed class CharacterTraitRowView : CharacterTraitRowViewBase
{
    [SerializeField]
    private Toggle _toggle;

    [SerializeField]
    private LocalizedTMPText _labelText;

    [SerializeField]
    private TMP_Text _labelValue;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    private readonly AsyncEvent<bool> _toggled = new();
    public override AsyncEvent<bool> Toggled => _toggled;

    public void Bind(
        Toggle toggle,
        LocalizedTMPText labelText,
        TMP_Text labelValue,
        CanvasGroup canvasGroup)
    {
        _toggle = toggle;
        _labelText = labelText;
        _labelValue = labelValue;
        _canvasGroup = canvasGroup;
    }

    public override void SetData(CharacterTraitRowData data)
    {
        _labelText.Key = data.LocalizationKey;
        _labelText.Fallback = data.FallbackLabel;
        _labelValue.text = data.FallbackLabel ?? string.Empty;
        _toggle.SetIsOnWithoutNotify(data.IsSelected);
        _toggle.interactable = data.IsInteractable;
    }

    public override void SetInteractable(bool isInteractable)
    {
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.blocksRaycasts = isInteractable;
        _toggle.interactable = isInteractable;
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
        StopSubscriptionsImmediate();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        StopSubscriptionsImmediate();
        return default;
    }

    private void SubscribeOnEvents()
    {
        StopSubscriptionsImmediate();
        _toggle.onValueChanged.AddListener(HandleToggleChanged);
    }

    private void StopSubscriptionsImmediate()
    {
        _toggle.onValueChanged.RemoveListener(HandleToggleChanged);
    }

    private void HandleToggleChanged(bool isSelected)
    {
        _toggled.InvokeAsync(isSelected).Forget();
    }
}
}
