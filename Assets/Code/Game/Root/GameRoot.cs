using System;
using System.IO;
using Code.Game.Bootstrap.State;
using Code.Game.Loading;
using Code.Game.MainMenu.States;
using Code.Generated.Addressables;
using Disposable;
using InGameLogger;
using LightDI.Runtime;
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

        var dispatcherObject = new GameObject();
        var unityDispatcherBehaviour = dispatcherObject.AddComponent<UnityDispatcherBehaviour>();

        var unityTickHandler = new UnityTickHandler(unityDispatcherBehaviour);
        _gameDiContainer.RegisterAsSingleton<ITickHandler>(unityTickHandler);
        
        var bootstrapState = new BootstrapState();
        _stateMachine.Register(bootstrapState);
        
        var loadingState = LoadingStateFactory.CreateLoadingState();
        _stateMachine.Register(loadingState);
        
        _stateMachine.Register(new MainMenuState());

        _gameDiContainer.RegisterAsSingleton(_stateMachine);

        var savePath = Path.Combine(Application.persistentDataPath, SavesFolderName);

        var unityBinaryLocalSaveSystem = new UnityBinaryLocalSaveSystem(savePath, 20);
        _gameDiContainer.RegisterAsSingleton<ILocalSaveSystem>(unityBinaryLocalSaveSystem);

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
                cancellationToken: destroyCancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected override void DisposeUnmanagedResources()
    {
        base.DisposeUnmanagedResources();

        _gameDiContainer.Dispose();
    }
}
}