using Code.Config.Pages.CharactersPage;
using Code.Game.Saves.Characters;

namespace Code.Game.MainMenu.Window
{
public enum AvatarSlideDirection
{
    None = 0,
    FromLeft = 1,
    FromRight = 2
}

public readonly struct CharacterStatRowData
{
    public CharacterStatRowData(
        CharacterStatType statType,
        string localizationKey,
        string fallbackLabel,
        int value,
        bool canIncrease,
        bool canDecrease)
    {
        StatType = statType;
        LocalizationKey = localizationKey;
        FallbackLabel = fallbackLabel;
        Value = value;
        CanIncrease = canIncrease;
        CanDecrease = canDecrease;
    }

    public CharacterStatType StatType { get; }
    public string LocalizationKey { get; }
    public string FallbackLabel { get; }
    public int Value { get; }
    public bool CanIncrease { get; }
    public bool CanDecrease { get; }
}

public readonly struct CharacterTraitRowData
{
    public CharacterTraitRowData(
        string traitId,
        string localizationKey,
        string fallbackLabel,
        bool isSelected,
        bool isInteractable)
    {
        TraitId = traitId;
        LocalizationKey = localizationKey;
        FallbackLabel = fallbackLabel;
        IsSelected = isSelected;
        IsInteractable = isInteractable;
    }

    public string TraitId { get; }
    public string LocalizationKey { get; }
    public string FallbackLabel { get; }
    public bool IsSelected { get; }
    public bool IsInteractable { get; }
}

public readonly struct CharacterTraitSelection
{
    public CharacterTraitSelection(string traitId, bool isSelected)
    {
        TraitId = traitId;
        IsSelected = isSelected;
    }

    public string TraitId { get; }
    public bool IsSelected { get; }
}

public readonly struct CharacterCreationResult
{
    public CharacterCreationResult(
        int slotIndex,
        string name,
        CharacterStats stats,
        string[] traitIds,
        string avatarId)
    {
        SlotIndex = slotIndex;
        Name = name;
        Stats = stats;
        TraitIds = traitIds;
        AvatarId = avatarId;
    }

    public int SlotIndex { get; }
    public string Name { get; }
    public CharacterStats Stats { get; }
    public string[] TraitIds { get; }
    public string AvatarId { get; }
}
}
