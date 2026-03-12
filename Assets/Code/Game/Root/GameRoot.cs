using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Azzazelloqq.Config;
using Code.Config;
using Code.Game.Bootstrap.State;
using Code.Game.Exploration.State;
using Code.Game.Flow;
using Code.Game.Loading;
using Code.Game.MainMenu.States;
using Code.Game.Saves;
using Code.Generated.Addressables;
using Cysharp.Threading.Tasks;
using Disposable;
using InGameLogger;
using LightDI.Runtime;
using LocalizedDomain;
using LocalizedDomain.Unity;
using LocalSaveSystem;
using Piggy.Code.StateMachine;
using ResourceLoader;
using ResourceLoader.AddressableResourceLoader;
using TickHandler;
using TickHandler.UnityTickHandler;
using UnityEngine;

namespace Code.Game.Root
{
public class GameRoot : MonoBehaviourDisposable
{
    private const string SavesFolderName = "Saves";

    [SerializeField]
    private RootContext _rootContext;

    [SerializeField]
    private TextAsset _localizationJson;

    [SerializeField]
    private string _defaultLocale = "en";

    [SerializeField]
    private List<string> _fallbackLocales = new() { "en" };

    [SerializeField]
    private MainConfig _mainConfig;

    private readonly IStateMachine _stateMachine = new StateMachine();
    private IDiContainer _gameDiContainer;
    private UnityInGameLogger _inGameLogger;
    private ISaveStore _saveStore;
    private IConfig _config;
    private IGameFlowService _gameFlowService;
    private IResourceLoader _resourceLoader;
    private Transform _gameplayRoot;


    // ReSharper disable once AsyncVoidMethod
    private async void Start()
    {
        _gameDiContainer = DiContainerFactory.CreateGlobalContainer();
        
        _resourceLoader = new AddressableResourceLoader();
        _gameDiContainer.RegisterAsSingleton<IResourceLoader>(_resourceLoader);

        _inGameLogger = new UnityInGameLogger();
        _gameDiContainer.RegisterAsSingleton<IInGameLogger>(_inGameLogger);
        
        await InitializeConfig(destroyCancellationToken);
        
        InitializeLocalization();
        InitializeSaveSystem();
        InitializeGameplayRoots();

        _gameFlowService = new GameFlowService(
            _stateMachine,
            _saveStore,
            _config,
            _resourceLoader,
            _rootContext.UIContext,
            _gameplayRoot);
        
        _gameDiContainer.RegisterAsSingleton(_gameFlowService);

        var dispatcherObject = new GameObject();
        var unityDispatcherBehaviour = dispatcherObject.AddComponent<UnityDispatcherBehaviour>();
        unityDispatcherBehaviour.name = $"[{unityDispatcherBehaviour.GetType().Name}]";
        DontDestroyOnLoad(unityDispatcherBehaviour);

        var unityTickHandler = new UnityTickHandler(unityDispatcherBehaviour);
        _gameDiContainer.RegisterAsSingleton<ITickHandler>(unityTickHandler);
        
        var bootstrapState = new BootstrapState();
        _stateMachine.Register(bootstrapState);
        
        var loadingState = LoadingStateFactory.CreateLoadingState();
        _stateMachine.Register(loadingState);

        var mainMenuState = MainMenuStateFactory.CreateMainMenuState();
        _stateMachine.Register(mainMenuState);

        var explorationState = new ExplorationState();
        _stateMachine.Register(explorationState);

        _gameDiContainer.RegisterAsSingleton(_stateMachine);

        try
        {
            await _stateMachine.ChangeStateAsync<BootstrapState, BootstrapStateContext>(
                new BootstrapStateContext(_rootContext.UIContext),
                cancellationToken: destroyCancellationToken);

            var loadingWindowViewResourceId = ResourceIdsContainer.UILoadingScreen.LoadingWindowView;
            var loadingStateContext = new LoadingStateContext(_rootContext.UIContext, loadingWindowViewResourceId);
            await _stateMachine.ChangeStateAsync<LoadingState, LoadingStateContext>(
                loadingStateContext,
                cancellationToken: destroyCancellationToken);

            var mainMenuSettingsView = ResourceIdsContainer.UIMainMenu.MainMenuSettingsView;

            var mainMenuStateContext = new MainMenuStateContext(_rootContext.UIContext, mainMenuSettingsView);
            await _stateMachine.ChangeStateAsync<MainMenuState, MainMenuStateContext>(
                mainMenuStateContext,
                transitionMode: StateTransitionMode.OverlapExitEnter,
                cancellationToken: destroyCancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _inGameLogger.LogException(exception);
        }
    }

    protected override void DisposeUnmanagedResources()
    {
        base.DisposeUnmanagedResources();

        _gameDiContainer.Dispose();
    }

    private void InitializeSaveSystem()
    {
        var savePath = Path.Combine(Application.persistentDataPath, SavesFolderName);
        var options = new SaveStoreOptions(savePath)
        {
            AutoSavePeriodSeconds = 20
        };

        _saveStore = new SaveStore(options);
        _saveStore.RegisterKeys(GameSaveKeys.All);
        _saveStore.StartAutoSave();

        _gameDiContainer.RegisterAsSingleton<ISaveStore>(_saveStore);
    }

    private void InitializeLocalization()
    {
        LocalizationRuntime.Initialize(_localizationJson, _defaultLocale, _fallbackLocales);

        ILocalizationProvider localizationProvider = LocalizationRuntime.Service;
        _gameDiContainer.RegisterAsSingleton(localizationProvider);
    }

    private void InitializeGameplayRoots()
    {
        _gameplayRoot = _rootContext.GameplayRoot;
        if (_gameplayRoot == null)
        {
            var gameplayRootObject = new GameObject("[GameplayRoot]");
            gameplayRootObject.transform.SetParent(transform, false);
            _gameplayRoot = gameplayRootObject.transform;
        }
    }

    private async UniTask InitializeConfig(CancellationToken token)
    {
        var scriptableObjectConfigParser = new ScriptableObjectConfigParser(_mainConfig);
        
        var config = new Azzazelloqq.Config.Config(scriptableObjectConfigParser);
        await config.InitializeAsync(token);
        _config = config;
        
        _gameDiContainer.RegisterAsSingleton<IConfig>(config);
    }
}
}