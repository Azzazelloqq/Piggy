using System;
using System.Threading;
using Code.Game.MainMenu.Window;
using Code.Game.Saves;
using Code.Game.Saves.Characters;
using Code.Game.Saves.Profile;
using Cysharp.Threading.Tasks;
using LocalSaveSystem;
using Piggy.Code.StateMachine;
using UnityEngine;

namespace Code.Game.MainMenu.States
{
public sealed class CharacterCreationSubState : GameSubState
{
    private MainMenuPresenter _presenter;
    private IMainMenuNavigator _navigator;
    private ISaveStore _saveStore;

    protected override UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
    {
        var context = (MainMenuSubStateContext)(object)gameStateContext;
        _presenter = context.Presenter;
        _navigator = context.Navigator;
        _saveStore = context.SaveStore;

        _presenter.CharacterCreationBackRequested.Subscribe(HandleBackRequested);
        _presenter.CharacterCreationCreateRequested.Subscribe(HandleCreateRequested);

        return UniTask.CompletedTask;
    }

    protected override UniTask OnExitAsync(CancellationToken cancellationToken)
    {
        _presenter.CharacterCreationBackRequested.Unsubscribe(HandleBackRequested);
        _presenter.CharacterCreationCreateRequested.Unsubscribe(HandleCreateRequested);

        return UniTask.CompletedTask;
    }

    private UniTask HandleBackRequested()
    {
        return _navigator.NavigateAsync(MainMenuScreen.Saves);
    }

    private UniTask HandleCreateRequested(CharacterCreationResult result)
    {
        CreateSaveForSlot(result);
        return _navigator.NavigateAsync(MainMenuScreen.Saves);
    }

    private void CreateSaveForSlot(CharacterCreationResult result)
    {
        if (_saveStore == null)
        {
            Debug.LogWarning("CharacterCreation: SaveStore is missing.");
            return;
        }

        var slotIndex = Math.Max(0, result.SlotIndex);
        var character = BuildPlayerCharacter(result);

        _saveStore.Update(GameSaveKeys.GameProfiles, (ref PlayerProfilesListSave save) =>
        {
            var saves = EnsureSaveSize(save.GameProfileSaves, slotIndex + 1);
            saves[slotIndex] = new GameProfileSave(new[] { character }, 0);
            save.GameProfileSaves = saves;
        });

        _saveStore.Save();
    }

    private static GameProfileSave[] EnsureSaveSize(GameProfileSave[] saves, int count)
    {
        var existing = saves ?? Array.Empty<GameProfileSave>();
        if (existing.Length >= count)
        {
            return existing;
        }

        var resized = new GameProfileSave[count];
        Array.Copy(existing, resized, existing.Length);
        return resized;
    }

    private static PlayerCharacter BuildPlayerCharacter(CharacterCreationResult result)
    {
        return new PlayerCharacter
        {
            CharacterId = Guid.NewGuid().ToString("N"),
            CharacterName = result.Name ?? string.Empty,
            CharacterLevel = 1,
            State = new CharacterState(),
            Skills = Array.Empty<CharacterSkill>(),
            Inventory = new CharacterInventory
            {
                InventoryItems = Array.Empty<string>(),
                InventoryCapacity = 0
            },
            Stats = result.Stats,
            Tags = new CharacterTags
            {
                TraitsIds = result.TraitIds ?? Array.Empty<string>()
            },
            CharacterView = new CharacterView
            {
                AvatarId = result.AvatarId ?? string.Empty
            }
        };
    }
}
}
