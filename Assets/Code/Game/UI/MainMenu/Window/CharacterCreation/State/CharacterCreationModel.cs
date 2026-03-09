using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Code.Config.Pages.CharactersPage;
using Code.Game.Async;
using Code.Game.Saves.Characters;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class CharacterCreationModel : CharacterCreationModelBase
{
    public override event Action<bool> VisibilityChanged;
    public override event Action StateChanged;

    public override AsyncEvent BackRequested { get; } = new();
    public override AsyncEvent<CharacterCreationResult> CreateRequested { get; } = new();

    private readonly Dictionary<CharacterStatType, int> _stats = new();
    private readonly HashSet<string> _selectedTraits = new(StringComparer.Ordinal);
    private readonly HashSet<string> _availableTraits = new(StringComparer.Ordinal);
    private readonly List<CharacterStatType> _statOrder = new();
    private readonly List<string> _avatarIds = new();
    private int _maxPoints;
    private int _maxTraits;
    private int _defaultStatValue;
    private bool _isVisible;
    private int _slotIndex;
    private int _avatarIndex;
    private string _name = string.Empty;

    public override bool IsVisible => _isVisible;
    public override string Name => _name;
    public override int SlotIndex => _slotIndex;
    public override int MaxPoints => Math.Max(0, _maxPoints);
    public override int UsedPoints => _stats.Values.Sum() - _defaultStatValue * _statOrder.Count;
    public override int MaxTraits => Math.Max(0, _maxTraits);
    public override int SelectedTraitsCount => _selectedTraits.Count;
    public override int AvatarIndex => _avatarIndex;

    public override bool CanCreate =>
        !string.IsNullOrWhiteSpace(_name)
        && (MaxPoints == 0 || UsedPoints == MaxPoints)
        && SelectedTraitsCount <= MaxTraits;

    public override void Configure(CharacterCreationModelConfig config)
    {
        _statOrder.Clear();
        foreach (var stat in config.StatOrder)
        {
            _statOrder.Add(stat);
        }

        _availableTraits.Clear();
        foreach (var traitId in config.TraitIds)
        {
            if (string.IsNullOrWhiteSpace(traitId))
            {
                throw new ArgumentException("Trait id is empty.", nameof(config));
            }

            _availableTraits.Add(traitId);
        }

        _avatarIds.Clear();
        foreach (var avatarId in config.AvatarIds)
        {
            _avatarIds.Add(avatarId ?? string.Empty);
        }

        _maxPoints = config.MaxPoints;
        _maxTraits = config.MaxTraits;
        _defaultStatValue = config.DefaultStatValue;

        InitializeStats();
        ClampAvatarIndex();
        NotifyStateChanged();
    }

    public override void SetSlotIndex(int slotIndex)
    {
        _slotIndex = Math.Max(0, slotIndex);
        ResetForNewCharacter();
        NotifyStateChanged();
    }

    public override void SetName(string name)
    {
        _name = name ?? string.Empty;
        NotifyStateChanged();
    }

    public override int GetStatValue(CharacterStatType type)
    {
        if (!_stats.TryGetValue(type, out var value))
        {
            throw new InvalidOperationException($"CharacterCreation: stat '{type}' is not configured.");
        }

        return value;
    }

    public override void IncreaseStat(CharacterStatType type)
    {
        if (UsedPoints >= MaxPoints)
        {
            return;
        }

        _stats[type] = GetStatValue(type) + 1;
        NotifyStateChanged();
    }

    public override void DecreaseStat(CharacterStatType type)
    {
        var value = GetStatValue(type);
        if (value <= 0)
        {
            return;
        }

        _stats[type] = value - 1;
        NotifyStateChanged();
    }

    public override bool IsTraitSelected(string traitId)
    {
        if (string.IsNullOrWhiteSpace(traitId))
        {
            throw new ArgumentException("Trait id is empty.", nameof(traitId));
        }

        if (!_availableTraits.Contains(traitId))
        {
            throw new InvalidOperationException($"CharacterCreation: trait '{traitId}' is not configured.");
        }

        return _selectedTraits.Contains(traitId);
    }

    public override void ToggleTrait(string traitId, bool isSelected)
    {
        if (string.IsNullOrWhiteSpace(traitId))
        {
            throw new ArgumentException("Trait id is empty.", nameof(traitId));
        }

        if (!_availableTraits.Contains(traitId))
        {
            throw new InvalidOperationException($"CharacterCreation: trait '{traitId}' is not configured.");
        }

        if (isSelected)
        {
            if (_selectedTraits.Contains(traitId))
            {
                return;
            }

            if (_selectedTraits.Count >= MaxTraits)
            {
                return;
            }

            _selectedTraits.Add(traitId);
        }
        else
        {
            _selectedTraits.Remove(traitId);
        }

        NotifyStateChanged();
    }

    public override void NextAvatar()
    {
        var count = _avatarIds.Count;
        if (count <= 0)
        {
            _avatarIndex = 0;
            return;
        }

        _avatarIndex = (_avatarIndex + 1) % count;
        NotifyStateChanged();
    }

    public override void PreviousAvatar()
    {
        var count = _avatarIds.Count;
        if (count <= 0)
        {
            _avatarIndex = 0;
            return;
        }

        _avatarIndex = (_avatarIndex - 1 + count) % count;
        NotifyStateChanged();
    }

    public override void Show()
    {
        _isVisible = true;
        VisibilityChanged?.Invoke(true);
    }

    public override void Hide()
    {
        _isVisible = false;
        VisibilityChanged?.Invoke(false);
    }

    public override UniTask RequestBackAsync()
    {
        return BackRequested.InvokeAsync();
    }

    public override UniTask RequestCreateAsync()
    {
        if (!CanCreate)
        {
            return UniTask.CompletedTask;
        }

        return CreateRequested.InvokeAsync(BuildResult());
    }

    public override CharacterCreationResult BuildResult()
    {
        return new CharacterCreationResult(
            _slotIndex,
            _name,
            BuildCharacterStats(),
            _selectedTraits.ToArray(),
            ResolveAvatarId());
    }

    protected override void OnInitialize()
    {
        InitializeStats();
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        InitializeStats();
        return default;
    }

    protected override void OnDispose()
    {
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        return default;
    }

    private void InitializeStats()
    {
        _stats.Clear();
        foreach (var type in _statOrder)
        {
            _stats[type] = _defaultStatValue;
        }
    }

    private void ResetForNewCharacter()
    {
        _name = string.Empty;
        _selectedTraits.Clear();

        var keys = _stats.Keys.ToArray();
        foreach (var key in keys)
        {
            _stats[key] = _defaultStatValue;
        }

        _avatarIndex = 0;
        ClampAvatarIndex();
    }

    private void ClampAvatarIndex()
    {
        var count = _avatarIds.Count;
        if (count <= 0)
        {
            _avatarIndex = 0;
            return;
        }

        if (_avatarIndex < 0)
        {
            _avatarIndex = 0;
        }
        else if (_avatarIndex >= count)
        {
            _avatarIndex = count - 1;
        }
    }

    private CharacterStats BuildCharacterStats()
    {
        var stats = new CharacterStats
        {
            Strength = GetStatValue(CharacterStatType.Strength),
            Dexterity = GetStatValue(CharacterStatType.Dexterity),
            Constitution = GetStatValue(CharacterStatType.Constitution),
            Wisdom = GetStatValue(CharacterStatType.Wisdom),
            Charisma = GetStatValue(CharacterStatType.Charisma),
            Intelligence = GetStatValue(CharacterStatType.Intelligence)
        };

        return stats;
    }

    private string ResolveAvatarId()
    {
        if (_avatarIds.Count == 0 || _avatarIndex < 0 || _avatarIndex >= _avatarIds.Count)
        {
            return string.Empty;
        }

        return _avatarIds[_avatarIndex];
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
}