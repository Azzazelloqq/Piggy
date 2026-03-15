using System;
using System.Threading;
using Azzazelloqq.Config;
using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Logic;
using Code.Game.Exploration.Runtime;
using Code.Game.Exploration.State;
using Code.Game.Exploration.View;
using Code.Game.Loading;
using Code.Game.Root;
using Code.Game.Saves;
using Code.Game.Saves.Profile;
using Code.Generated.Addressables;
using Cysharp.Threading.Tasks;
using LocalSaveSystem;
using Piggy.Code.StateMachine;
using ResourceLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.Flow
{
public sealed class GameFlowService : IGameFlowService
{
    private readonly IStateMachine _stateMachine;
    private readonly ISaveStore _saveStore;
    private readonly IConfig _config;
    private readonly IResourceLoader _resourceLoader;
    private readonly UIContext _uiContext;
    private readonly Transform _gameplayRoot;
    private Transform _sceneRoot;
    private readonly ExplorationTimeTicker _timeTicker;

    public GameFlowService(
        IStateMachine stateMachine,
        ISaveStore saveStore,
        IConfig config,
        IResourceLoader resourceLoader,
        UIContext uiContext,
        Transform gameplayRoot,
        ExplorationTimeTicker timeTicker)
    {
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        _saveStore = saveStore ?? throw new ArgumentNullException(nameof(saveStore));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _resourceLoader = resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader));
        _uiContext = uiContext ?? throw new ArgumentNullException(nameof(uiContext));
        _gameplayRoot = gameplayRoot ?? throw new ArgumentNullException(nameof(gameplayRoot));
        _timeTicker = timeTicker ?? throw new ArgumentNullException(nameof(timeTicker));
    }

    public int ActiveSlotIndex { get; private set; } = -1;

    public async UniTask StartGameAsync(int slotIndex, CancellationToken cancellationToken = default)
    {
        var profile = GetRequiredProfile(slotIndex);
        ActiveSlotIndex = slotIndex;
        var loadingOperation = new ExplorationLoadOperation(this, profile);
        var loadingViewId = ResourceIdsContainer.UILoadingScreen.LoadingWindowView;
        var loadingContext = new LoadingStateContext(
            _uiContext,
            loadingViewId,
            totalSteps: 2,
            stepsOperation: loadingOperation.ExecuteAsync);

        try
        {
            await _stateMachine.ChangeStateAsync<LoadingState, LoadingStateContext>(
                loadingContext,
                cancellationToken: cancellationToken);

            var loadedExploration = loadingOperation.GetResult();
            var explorationContext = new ExplorationStateContext(
                _uiContext,
                loadedExploration.Session,
                loadedExploration.LocationView);
            await _stateMachine.ChangeStateAsync<ExplorationState, ExplorationStateContext>(
                explorationContext,
                transitionMode: StateTransitionMode.OverlapExitEnter,
                cancellationToken: cancellationToken);

            UpdateProfile(slotIndex, loadedExploration.Session);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private GameProfileSave GetRequiredProfile(int slotIndex)
    {
        var profiles = _saveStore.Get(GameSaveKeys.GameProfiles);
        var saves = profiles.GameProfileSaves ?? Array.Empty<GameProfileSave>();
        if (slotIndex < 0 || slotIndex >= saves.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(slotIndex), slotIndex, "Save slot index is out of range.");
        }

        var profile = saves[slotIndex];
        if (profile.PlayerCharacters == null || profile.PlayerCharacters.Length == 0)
        {
            throw new InvalidOperationException($"Save slot {slotIndex} does not contain any playable characters.");
        }

        return profile;
    }

    private ExplorationSession BuildSession(GameProfileSave profile)
    {
        var worldConfig = GetRequiredConfigPage<WorldConfigPage>();
        var locationsConfig = GetRequiredConfigPage<LocationsConfigPage>();
        var locationLookup = locationsConfig.BuildLookup();
        var worldState = WorldStateMapper.ToRuntime(profile.WorldState, worldConfig, locationLookup);
        if (worldConfig.TimeUnitMinutes <= 0)
        {
            throw new InvalidOperationException("WorldConfigPage.TimeUnitMinutes must be greater than 0.");
        }

        var conditionEvaluator = new ConditionEvaluator();
        var actionExecutor = new ActionExecutor();
        var eventResolver = new EventResolver(conditionEvaluator);
        var runtimeServices = new ExplorationRuntimeServices(
            actionExecutor,
            conditionEvaluator,
            eventResolver,
            new D20CheckService());
        var timeService = new TimeService(worldConfig.TimeUnitMinutes, worldState.CurrentTimeUnits);
        var timeController = new ExplorationTimeController(worldState, timeService, eventResolver, actionExecutor);

        return new ExplorationSession(
            worldState,
            locationLookup,
            timeService,
            runtimeServices,
            timeController,
            worldConfig.DefaultFlowUnitsPerSecond);
    }

    private void UpdateProfile(int slotIndex, ExplorationSession session)
    {
        var locationName = ResolveLocationName(session);
        var inGameTimeText = session.TimeService.FormatCurrentTime();
        var worldStateSave = WorldStateMapper.ToSave(session.WorldState);

        _saveStore.Update(GameSaveKeys.GameProfiles, (ref PlayerProfilesListSave save) =>
        {
            var saves = save.GameProfileSaves ?? Array.Empty<GameProfileSave>();
            if (slotIndex < 0 || slotIndex >= saves.Length)
            {
                throw new InvalidOperationException($"Save slot {slotIndex} is no longer available.");
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
        if (!string.IsNullOrWhiteSpace(session.CurrentLocationConfig.DisplayName))
        {
            return session.CurrentLocationConfig.DisplayName;
        }

        return session.WorldState.CurrentLocationId;
    }

    private async UniTask<ExplorationLocationView> LoadLocationViewAsync(
        ExplorationSession session,
        CancellationToken token)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        var locationResourceId = session.CurrentLocationConfig.PrefabAddress;
        if (string.IsNullOrWhiteSpace(locationResourceId))
        {
            throw new InvalidOperationException(
                $"Location '{session.CurrentLocationConfig.Id}' does not define a prefab address.");
        }

        var parent = EnsureSceneRoot();
        var locationView = await _resourceLoader.LoadAndCreateAsync<ExplorationLocationView, Transform>(
            locationResourceId,
            parent,
            token);
        if (locationView == null)
        {
            throw new InvalidOperationException($"Failed to load exploration location view '{locationResourceId}'.");
        }

        ValidateLoadedLocationView(session, locationView);
        return locationView;
    }

    private Transform EnsureSceneRoot()
    {
        if (_sceneRoot != null)
        {
            Object.Destroy(_sceneRoot.gameObject);
            _sceneRoot = null;
        }

        var sceneRootObject = new GameObject("[SceneRoot]");
        sceneRootObject.transform.SetParent(_gameplayRoot, false);
        _sceneRoot = sceneRootObject.transform;
        return _sceneRoot;
    }

    private TPage GetRequiredConfigPage<TPage>() where TPage : class, IConfigPage
    {
        var page = _config.GetConfigPage<TPage>();
        if (page == null)
        {
            throw new InvalidOperationException($"{typeof(TPage).Name} is missing from the game config.");
        }

        return page;
    }

    private static void ValidateLoadedLocationView(ExplorationSession session, ExplorationLocationView locationView)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (locationView == null)
        {
            throw new ArgumentNullException(nameof(locationView));
        }

        if (!string.IsNullOrWhiteSpace(locationView.LocationId) &&
            !string.Equals(locationView.LocationId, session.CurrentLocationConfig.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Loaded location view '{locationView.LocationId}' does not match location '{session.CurrentLocationConfig.Id}'.");
        }
    }

    private readonly struct LoadedExplorationScene
    {
        public LoadedExplorationScene(
            ExplorationSession session,
            ExplorationLocationView locationView)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            LocationView = locationView ?? throw new ArgumentNullException(nameof(locationView));
        }

        public ExplorationSession Session { get; }
        public ExplorationLocationView LocationView { get; }
    }

    private sealed class ExplorationLoadOperation
    {
        private readonly GameFlowService _owner;
        private readonly GameProfileSave _profile;
        private LoadedExplorationScene _result;
        private bool _hasResult;

        public ExplorationLoadOperation(GameFlowService owner, GameProfileSave profile)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _profile = profile;
        }

        public async UniTask ExecuteAsync(ILoadingStepReporter reporter, CancellationToken token)
        {
            var session = _owner.BuildSession(_profile);
            _owner.BindTimeTicker(session);
            reporter.CompleteStep();

            var locationView = await _owner.LoadLocationViewAsync(session, token);
            reporter.CompleteStep();

            _result = new LoadedExplorationScene(session, locationView);
            _hasResult = true;
            await UniTask.Yield(token);
        }

        public LoadedExplorationScene GetResult()
        {
            if (!_hasResult)
            {
                throw new InvalidOperationException("Exploration loading did not produce a result.");
            }

            return _result;
        }
    }

    private void BindTimeTicker(ExplorationSession session)
    {
        if (session == null)
        {
            return;
        }

        _timeTicker.Bind(session.TimeController);
    }
}
}