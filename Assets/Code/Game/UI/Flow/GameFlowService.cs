using System;
using System.Threading;
using Azzazelloqq.Config;
using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Runtime;
using Code.Game.Exploration.State;
using Code.Game.Loading;
using Code.Game.Root;
using Code.Game.Saves;
using Code.Game.Saves.Profile;
using Code.Generated.Addressables;
using Cysharp.Threading.Tasks;
using InGameLogger;
using LocalSaveSystem;
using Piggy.Code.StateMachine;
using UnityEngine;

namespace Code.Game.Flow
{
public sealed class GameFlowService : IGameFlowService
{
    private readonly IStateMachine _stateMachine;
    private readonly ISaveStore _saveStore;
    private readonly IConfig _config;
    private readonly UIContext _uiContext;
    private readonly IInGameLogger _logger;

    public GameFlowService(
        IStateMachine stateMachine,
        ISaveStore saveStore,
        IConfig config,
        UIContext uiContext,
        IInGameLogger logger)
    {
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        _saveStore = saveStore ?? throw new ArgumentNullException(nameof(saveStore));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _uiContext = uiContext;
        _logger = logger;
    }

    public int ActiveSlotIndex { get; private set; } = -1;

    public async UniTask StartGameAsync(int slotIndex, CancellationToken cancellationToken = default)
    {
        ActiveSlotIndex = slotIndex;

        if (!TryGetProfile(slotIndex, out var profile))
        {
            return;
        }

        ExplorationSession session = null;
        var loadingViewId = ResourceIdsContainer.UILoadingScreen.LoadingWindowView;
        var loadingContext = new LoadingStateContext(
            _uiContext,
            loadingViewId,
            totalSteps: 2,
            stepsOperation: async (reporter, token) =>
            {
                session = BuildSession(profile);
                reporter.CompleteStep();
                UpdateProfile(slotIndex, session);
                reporter.CompleteStep();
                await UniTask.Yield(token);
            });

        try
        {
            await _stateMachine.ChangeStateAsync<LoadingState, LoadingStateContext>(
                loadingContext,
                cancellationToken: cancellationToken);

            if (session == null)
            {
                return;
            }

            var explorationContext = new ExplorationStateContext(_uiContext, session);
            await _stateMachine.ChangeStateAsync<ExplorationState, ExplorationStateContext>(
                explorationContext,
                transitionMode: StateTransitionMode.OverlapExitEnter,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
        }
    }

    private bool TryGetProfile(int slotIndex, out GameProfileSave profile)
    {
        profile = default;
        var profiles = _saveStore.Get(GameSaveKeys.GameProfiles);
        var saves = profiles.GameProfileSaves ?? Array.Empty<GameProfileSave>();
        if (slotIndex < 0 || slotIndex >= saves.Length)
        {
            Debug.LogWarning($"GameFlowService: Save slot {slotIndex} is out of range.");
            return false;
        }

        profile = saves[slotIndex];
        if (profile.PlayerCharacters == null || profile.PlayerCharacters.Length == 0)
        {
            Debug.LogWarning($"GameFlowService: Save slot {slotIndex} has no characters.");
            return false;
        }

        return true;
    }

    private ExplorationSession BuildSession(GameProfileSave profile)
    {
        var worldConfig = _config.GetConfigPage<WorldConfigPage>();
        var locationsConfig = _config.GetConfigPage<LocationsConfigPage>();
        var worldState = WorldStateMapper.ToRuntime(profile.WorldState, worldConfig, locationsConfig);

        var currentLocationConfig = locationsConfig?.FindLocation(worldState.CurrentLocationId);
        worldState.Locations.TryGetValue(worldState.CurrentLocationId, out var currentLocationState);

        var minutesPerUnit = worldConfig != null ? worldConfig.TimeUnitMinutes : 10;
        var timeService = new TimeService(minutesPerUnit, worldState.CurrentTimeUnits);

        return new ExplorationSession(worldState, currentLocationConfig, currentLocationState, timeService);
    }

    private void UpdateProfile(int slotIndex, ExplorationSession session)
    {
        if (session == null)
        {
            return;
        }

        var locationName = ResolveLocationName(session);
        var inGameTimeText = session.TimeService?.FormatCurrentTime() ?? string.Empty;
        var worldStateSave = WorldStateMapper.ToSave(session.WorldState);

        _saveStore.Update(GameSaveKeys.GameProfiles, (ref PlayerProfilesListSave save) =>
        {
            var saves = save.GameProfileSaves ?? Array.Empty<GameProfileSave>();
            if (slotIndex < 0 || slotIndex >= saves.Length)
            {
                return;
            }

            var profile = saves[slotIndex];
            profile.WorldState = worldStateSave;
            profile.LastLocationName = locationName;
            profile.InGameTimeText = inGameTimeText;
            saves[slotIndex] = profile;
            save.GameProfileSaves = saves;
        });

        _saveStore.Save();
    }

    private static string ResolveLocationName(ExplorationSession session)
    {
        if (session.CurrentLocationConfig != null &&
            !string.IsNullOrWhiteSpace(session.CurrentLocationConfig.DisplayName))
        {
            return session.CurrentLocationConfig.DisplayName;
        }

        return session.WorldState?.CurrentLocationId ?? string.Empty;
    }
}
}
