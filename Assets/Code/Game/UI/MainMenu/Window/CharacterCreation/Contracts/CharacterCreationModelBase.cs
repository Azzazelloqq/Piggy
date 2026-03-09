using System;
using Code.Config.Pages.CharactersPage;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class CharacterCreationModelBase : Model
{
    public abstract event Action<bool> VisibilityChanged;
    public abstract event Action StateChanged;

    public abstract AsyncEvent BackRequested { get; }
    public abstract AsyncEvent<CharacterCreationResult> CreateRequested { get; }

    public abstract bool IsVisible { get; }
    public abstract string Name { get; }
    public abstract int SlotIndex { get; }
    public abstract int MaxPoints { get; }
    public abstract int UsedPoints { get; }
    public abstract int MaxTraits { get; }
    public abstract int SelectedTraitsCount { get; }
    public abstract int AvatarIndex { get; }
    public abstract bool CanCreate { get; }
    public abstract void Configure(CharacterCreationModelConfig config);
    public abstract void SetSlotIndex(int slotIndex);
    public abstract void SetName(string name);
    public abstract int GetStatValue(CharacterStatType type);
    public abstract void IncreaseStat(CharacterStatType type);
    public abstract void DecreaseStat(CharacterStatType type);
    public abstract bool IsTraitSelected(string traitId);
    public abstract void ToggleTrait(string traitId, bool isSelected);
    public abstract void NextAvatar();
    public abstract void PreviousAvatar();
    public abstract void Show();
    public abstract void Hide();
    public abstract UniTask RequestBackAsync();
    public abstract UniTask RequestCreateAsync();
    public abstract CharacterCreationResult BuildResult();
}
}
