using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.Config;
using Code.Config.Pages.CharactersPage;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using LightDI.Runtime;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public sealed class CharacterCreationPresenter : CharacterCreationPresenterBase
{
    private const string DefaultConfigResourcePath = "Config/CharactersConfigPage";
    private readonly List<CharacterStatRowPresenter> _statPresenters = new();
    private readonly List<CharacterTraitRowPresenter> _traitPresenters = new();
    private CharacterStatConfig[] _statDefinitions = Array.Empty<CharacterStatConfig>();
    private CharacterTagsConfig[] _traitDefinitions = Array.Empty<CharacterTagsConfig>();
    private CharacterClassConfig[] _classDefinitions = Array.Empty<CharacterClassConfig>();
    private CharacterAvatarConfig[] _avatarDefinitions = Array.Empty<CharacterAvatarConfig>();
    private bool _configPrepared;
    private readonly CharactersConfigPage _characterConfigPage;
    private AvatarSlideDirection _pendingAvatarSlideDirection = AvatarSlideDirection.None;
    private int _lastAvatarIndex = -1;

    public override AsyncEvent BackRequested { get; } = new();
    public override AsyncEvent<CharacterCreationResult> CreateRequested { get; } = new();

    public CharacterCreationPresenter(CharacterCreationViewBase view, CharacterCreationModelBase model, [Inject] IConfig config)
        : base(view, model)
    {
        _characterConfigPage = config.GetConfigPage<CharactersConfigPage>();
    }

    public override void Show()
    {
        model.Show();
        view.SetVisible(model.IsVisible);
        SyncView();
    }

    public override void Hide()
    {
        model.Hide();
        view.SetVisible(model.IsVisible);
    }

    public override void PrepareSlot(int slotIndex)
    {
        model.SetSlotIndex(slotIndex);
        view.SetName(model.Name);
        _lastAvatarIndex = -1;
        _pendingAvatarSlideDirection = AvatarSlideDirection.None;
        SyncView();
    }

    public override UniTask RequestBackAsync()
    {
        return model.RequestBackAsync();
    }

    public override UniTask RequestCreateAsync()
    {
        return model.RequestCreateAsync();
    }

    protected override void OnInitialize()
    {
        SubscribeOnEvents();
        PrepareConfig();
        model.Configure(BuildModelConfig());
        SyncView();
        view.SetVisible(model.IsVisible);
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeOnEvents();
        PrepareConfig();
        model.Configure(BuildModelConfig());
        SyncView();
        view.SetVisible(model.IsVisible);
        return default;
    }

    protected override void OnDispose()
    {
        UnsubscribeOnEvents();
        DisposeStatPresenters();
        DisposeTraitPresenters();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeOnEvents();
        DisposeStatPresenters();
        DisposeTraitPresenters();
        return default;
    }

    private void SubscribeOnEvents()
    {
        view.BackClicked.Subscribe(HandleBackClicked);
        view.CreateClicked.Subscribe(HandleCreateClicked);
        view.NameChanged.Subscribe(HandleNameChanged);
        view.AvatarPrevClicked.Subscribe(HandleAvatarPrevClicked);
        view.AvatarNextClicked.Subscribe(HandleAvatarNextClicked);

        model.BackRequested.Subscribe(HandleBackRequested);
        model.CreateRequested.Subscribe(HandleCreateRequested);
        model.StateChanged += HandleStateChanged;
    }

    private void UnsubscribeOnEvents()
    {
        view.BackClicked.Unsubscribe(HandleBackClicked);
        view.CreateClicked.Unsubscribe(HandleCreateClicked);
        view.NameChanged.Unsubscribe(HandleNameChanged);
        view.AvatarPrevClicked.Unsubscribe(HandleAvatarPrevClicked);
        view.AvatarNextClicked.Unsubscribe(HandleAvatarNextClicked);

        model.BackRequested.Unsubscribe(HandleBackRequested);
        model.CreateRequested.Unsubscribe(HandleCreateRequested);
        model.StateChanged -= HandleStateChanged;
    }

    private UniTask HandleBackClicked()
    {
        return model.RequestBackAsync();
    }

    private UniTask HandleCreateClicked()
    {
        return model.RequestCreateAsync();
    }

    private UniTask HandleNameChanged(string name)
    {
        model.SetName(name);
        return UniTask.CompletedTask;
    }

    private UniTask HandleAvatarPrevClicked()
    {
        _pendingAvatarSlideDirection = AvatarSlideDirection.FromLeft;
        model.PreviousAvatar();
        return UniTask.CompletedTask;
    }

    private UniTask HandleAvatarNextClicked()
    {
        _pendingAvatarSlideDirection = AvatarSlideDirection.FromRight;
        model.NextAvatar();
        return UniTask.CompletedTask;
    }

    private UniTask HandleBackRequested()
    {
        return BackRequested.InvokeAsync();
    }

    private UniTask HandleCreateRequested(CharacterCreationResult result)
    {
        return CreateRequested.InvokeAsync(result);
    }

    private void HandleStateChanged()
    {
        SyncView();
    }

    private void SyncView()
    {
        SyncStats();
        SyncTraits();
        SyncAvatar();
        SyncCounters();
        view.SetCreateInteractable(model.CanCreate);
    }

    private void SyncStats()
    {
        var stats = _statDefinitions;
        var views = view.EnsureStatRows(stats.Length);
        EnsureStatPresenters(views);

        for (var i = 0; i < stats.Length; i++)
        {
            var definition = stats[i];
            var statType = definition.Type;
            var value = model.GetStatValue(statType);
            var canIncrease = model.UsedPoints < model.MaxPoints;
            var canDecrease = value > 0;
            var data = new CharacterStatRowData(
                statType,
                definition.LocalisationKey,
                definition.FallbackLabel,
                value,
                canIncrease,
                canDecrease);

            _statPresenters[i].SetData(data);
        }
    }

    private void SyncTraits()
    {
        var traits = _traitDefinitions;
        var views = view.EnsureTraitRows(traits.Length);
        EnsureTraitPresenters(views);

        var selectedCount = model.SelectedTraitsCount;
        var maxTraits = Math.Max(0, model.MaxTraits);

        for (var i = 0; i < traits.Length; i++)
        {
            var trait = traits[i];
            var isSelected = model.IsTraitSelected(trait.Id);
            var canSelect = isSelected || selectedCount < maxTraits;
            var data = new CharacterTraitRowData(
                trait.Id,
                trait.LocalisationKey,
                trait.Id,
                isSelected,
                canSelect);

            _traitPresenters[i].SetData(data);
        }
    }

    private void SyncAvatar()
    {
        var classes = _classDefinitions;
        if (classes.Length == 0)
        {
            _lastAvatarIndex = -1;
            _pendingAvatarSlideDirection = AvatarSlideDirection.None;
            view.SetAvatar(null, string.Empty, "Нет классов", AvatarSlideDirection.None);
            return;
        }

        var index = model.AvatarIndex;
        if (index < 0 || index >= classes.Length)
        {
            index = 0;
        }

        var characterClass = classes[index];
        var label = !string.IsNullOrWhiteSpace(characterClass.Id)
            ? characterClass.Id
            : $"Класс {index + 1}";
        var portrait = ResolveAvatarPortrait(characterClass.AvatarId);
        var direction = AvatarSlideDirection.None;
        if (_lastAvatarIndex >= 0 && _lastAvatarIndex != index)
        {
            direction = _pendingAvatarSlideDirection;
            if (direction == AvatarSlideDirection.None)
            {
                direction = index > _lastAvatarIndex
                    ? AvatarSlideDirection.FromRight
                    : AvatarSlideDirection.FromLeft;
            }
        }

        view.SetAvatar(portrait, characterClass.LocalisationKey, label, direction);
        _pendingAvatarSlideDirection = AvatarSlideDirection.None;
        _lastAvatarIndex = index;
    }

    private void SyncCounters()
    {
        var pointsLeft = model.MaxPoints - model.UsedPoints;
        var pointsText = $"Очки: {pointsLeft}/{model.MaxPoints}";
        var traitsText = $"Выбрано: {model.SelectedTraitsCount}/{model.MaxTraits}";
        view.SetPointsText(pointsText);
        view.SetTraitsText(traitsText);
    }

    private void EnsureStatPresenters(IReadOnlyList<CharacterStatRowViewBase> views)
    {
        for (var i = _statPresenters.Count; i < views.Count; i++)
        {
            var presenter = new CharacterStatRowPresenter(views[i], new CharacterStatRowModel());
            presenter.IncreaseRequested.Subscribe(HandleStatIncreaseRequested);
            presenter.DecreaseRequested.Subscribe(HandleStatDecreaseRequested);
            presenter.Initialize();
            _statPresenters.Add(presenter);
        }

        for (var i = _statPresenters.Count - 1; i >= views.Count; i--)
        {
            var presenter = _statPresenters[i];
            presenter.IncreaseRequested.Unsubscribe(HandleStatIncreaseRequested);
            presenter.DecreaseRequested.Unsubscribe(HandleStatDecreaseRequested);
            presenter.Dispose();
            _statPresenters.RemoveAt(i);
        }
    }

    private void EnsureTraitPresenters(IReadOnlyList<CharacterTraitRowViewBase> views)
    {
        for (var i = _traitPresenters.Count; i < views.Count; i++)
        {
            var presenter = new CharacterTraitRowPresenter(views[i], new CharacterTraitRowModel());
            presenter.SelectionChanged.Subscribe(HandleTraitSelectionChanged);
            presenter.Initialize();
            _traitPresenters.Add(presenter);
        }

        for (var i = _traitPresenters.Count - 1; i >= views.Count; i--)
        {
            var presenter = _traitPresenters[i];
            presenter.SelectionChanged.Unsubscribe(HandleTraitSelectionChanged);
            presenter.Dispose();
            _traitPresenters.RemoveAt(i);
        }
    }

    private void DisposeStatPresenters()
    {
        for (var i = 0; i < _statPresenters.Count; i++)
        {
            var presenter = _statPresenters[i];
            presenter.IncreaseRequested.Unsubscribe(HandleStatIncreaseRequested);
            presenter.DecreaseRequested.Unsubscribe(HandleStatDecreaseRequested);
            presenter.Dispose();
        }

        _statPresenters.Clear();
    }

    private void DisposeTraitPresenters()
    {
        for (var i = 0; i < _traitPresenters.Count; i++)
        {
            var presenter = _traitPresenters[i];
            presenter.SelectionChanged.Unsubscribe(HandleTraitSelectionChanged);
            presenter.Dispose();
        }

        _traitPresenters.Clear();
    }

    private UniTask HandleStatIncreaseRequested(CharacterStatType statType)
    {
        model.IncreaseStat(statType);
        return UniTask.CompletedTask;
    }

    private UniTask HandleStatDecreaseRequested(CharacterStatType statType)
    {
        model.DecreaseStat(statType);
        return UniTask.CompletedTask;
    }

    private UniTask HandleTraitSelectionChanged(CharacterTraitSelection selection)
    {
        model.ToggleTrait(selection.TraitId, selection.IsSelected);
        return UniTask.CompletedTask;
    }

    private void PrepareConfig()
    {
        if (_configPrepared)
        {
            return;
        }

        if (_characterConfigPage == null)
        {
            throw new InvalidOperationException("CharacterCreation: missing CharactersConfigPage resource.");
        }

        _statDefinitions = _characterConfigPage.StatsConfig.Stats ?? Array.Empty<CharacterStatConfig>();
        if (_statDefinitions.Length == 0)
        {
            throw new InvalidOperationException("CharacterCreation: stats config is empty.");
        }

        _traitDefinitions = _characterConfigPage.AllTraitsIds ?? Array.Empty<CharacterTagsConfig>();
        _classDefinitions = _characterConfigPage.Classes ?? Array.Empty<CharacterClassConfig>();
        _avatarDefinitions = _characterConfigPage.Avatars ?? Array.Empty<CharacterAvatarConfig>();
        _configPrepared = true;
    }

    private CharacterCreationModelConfig BuildModelConfig()
    {
        var statOrder = new CharacterStatType[_statDefinitions.Length];
        for (var i = 0; i < _statDefinitions.Length; i++)
        {
            statOrder[i] = _statDefinitions[i].Type;
        }

        var traitIds = new string[_traitDefinitions.Length];
        for (var i = 0; i < _traitDefinitions.Length; i++)
        {
            var traitId = _traitDefinitions[i].Id;
            if (string.IsNullOrWhiteSpace(traitId))
            {
                throw new InvalidOperationException("CharacterCreation: trait id is empty.");
            }

            traitIds[i] = traitId;
        }

        var avatarIds = new string[_classDefinitions.Length];
        for (var i = 0; i < _classDefinitions.Length; i++)
        {
            avatarIds[i] = _classDefinitions[i].AvatarId ?? string.Empty;
        }

        var maxSumPoints = _characterConfigPage.StatsConfig.MaxSumCharacterPoints;
        var defaultStatValue = _characterConfigPage.StatsConfig.DefaultStatValue;
        var maxTraits = _characterConfigPage.MaxTraits;
        return new CharacterCreationModelConfig(
            statOrder,
            traitIds,
            avatarIds,
            maxSumPoints,
            maxTraits,
            defaultStatValue);
    }

    private Sprite ResolveAvatarPortrait(string avatarId)
    {
        if (TryGetAvatarDefinition(avatarId, out var avatar))
        {
            return avatar.Portrait;
        }

        return null;
    }

    private bool TryGetAvatarDefinition(string avatarId, out CharacterAvatarConfig avatar)
    {
        if (string.IsNullOrWhiteSpace(avatarId))
        {
            avatar = default;
            return false;
        }

        for (var i = 0; i < _avatarDefinitions.Length; i++)
        {
            var candidate = _avatarDefinitions[i];
            if (string.Equals(candidate.Id, avatarId, StringComparison.Ordinal))
            {
                avatar = candidate;
                return true;
            }
        }

        avatar = default;
        return false;
    }
}
}
