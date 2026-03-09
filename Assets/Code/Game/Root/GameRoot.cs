using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Azzazelloqq.Config;
using Code.Config;
using Code.Game.Bootstrap.State;
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


    // ReSharper disable once AsyncVoidMethod
    private async void Start()
    {
        _gameDiContainer = DiContainerFactory.CreateGlobalContainer();
        
        var resourceLoader = new AddressableResourceLoader();
        _gameDiContainer.RegisterAsSingleton<IResourceLoader>(resourceLoader);

        _inGameLogger = new UnityInGameLogger();
        _gameDiContainer.RegisterAsSingleton<IInGameLogger>(_inGameLogger);
        
        await InitializeConfig(destroyCancellationToken);
        
        InitializeLocalization();
        InitializeSaveSystem();

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

        var saveStore = new SaveStore(options);
        saveStore.RegisterKeys(GameSaveKeys.All);
        saveStore.StartAutoSave();

        _gameDiContainer.RegisterAsSingleton<ISaveStore>(saveStore);
    }

    private void InitializeLocalization()
    {
        LocalizationRuntime.Initialize(_localizationJson, _defaultLocale, _fallbackLocales);

        ILocalizationProvider localizationProvider = LocalizationRuntime.Service;
        _gameDiContainer.RegisterAsSingleton(localizationProvider);
    }

    private async UniTask InitializeConfig(CancellationToken token)
    {
        var scriptableObjectConfigParser = new ScriptableObjectConfigParser(_mainConfig);
        
        var config = new Azzazelloqq.Config.Config(scriptableObjectConfigParser);
        await config.InitializeAsync(token);
        
        _gameDiContainer.RegisterAsSingleton<IConfig>(config);
    }
}
}