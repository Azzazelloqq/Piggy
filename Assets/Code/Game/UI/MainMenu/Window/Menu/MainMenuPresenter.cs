using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;
using LightDI.Runtime;
using LocalSaveSystem;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuPresenter : MainMenuPresenterBase
{
    private readonly SettingsPopupPresenter _settingsPopupPresenter;
    private readonly ExitConfirmPopupPresenter _exitPopupPresenter;
    private readonly GameProfilesPresenter _savesPresenter;
    private readonly CharacterCreationPresenter _characterCreationPresenter;
    private readonly MainMenuScreenTransitionPresenter _transitionPresenter;

    public AsyncEvent SettingsBackRequested { get; } = new();
    public AsyncEvent SettingsApplyRequested { get; } = new();
    public AsyncEvent ExitConfirmed { get; } = new();
    public AsyncEvent ExitCanceled { get; } = new();
    public AsyncEvent SavesBackRequested { get; } = new();
    public AsyncEvent<GameProfilesSlotData> SaveSlotSelected { get; } = new();
    public AsyncEvent CharacterCreationBackRequested { get; } = new();
    public AsyncEvent<CharacterCreationResult> CharacterCreationCreateRequested { get; } = new();

    public MainMenuPresenter(MainMenuViewBase view, MainMenuModelBase model, [Inject] ISaveStore saveStore)
        : base(view, model)
    {
        var settingsViewBase = view.SettingsPopupView;
        var exitConfirmViewBase = view.ExitConfirmPopupView;
        var savesViewBase = view.SavesView;

        var settingsModel = new SettingsPopupModel();
        var exitModel = new ExitConfirmPopupModel();
        var savesModel = new GameProfilesModel(saveStore);
        var creationModel = new CharacterCreationModel();

        _settingsPopupPresenter = new SettingsPopupPresenter(settingsViewBase, settingsModel);
        _exitPopupPresenter = new ExitConfirmPopupPresenter(exitConfirmViewBase, exitModel);
        _savesPresenter = new GameProfilesPresenter(savesViewBase, savesModel);
        _characterCreationPresenter =
            CharacterCreationPresenterFactory.CreateCharacterCreationPresenter(view.CharacterCreationView,
                creationModel);

        compositeDisposable.AddDisposable(_settingsPopupPresenter);
        compositeDisposable.AddDisposable(_exitPopupPresenter);
        compositeDisposable.AddDisposable(_savesPresenter);
        compositeDisposable.AddDisposable(_characterCreationPresenter);

        var menuPanel = new MainMenuScreenTransitionView.PanelHandle(
            view.Panel,
            Show,
            Hide,
            view.SetInteractable,
            view.AnimatedElements);
        var settingsPanel = new MainMenuScreenTransitionView.PanelHandle(
            settingsViewBase.Panel,
            _settingsPopupPresenter.Show,
            _settingsPopupPresenter.Hide,
            settingsViewBase.SetInteractable,
            settingsViewBase.AnimatedElements);
        var exitPanel = new MainMenuScreenTransitionView.PanelHandle(
            exitConfirmViewBase.Panel,
            _exitPopupPresenter.Show,
            _exitPopupPresenter.Hide,
            exitConfirmViewBase.SetInteractable,
            exitConfirmViewBase.AnimatedElements);
        var savesPanel = new MainMenuScreenTransitionView.PanelHandle(
            savesViewBase.Panel,
            _savesPresenter.Show,
            _savesPresenter.Hide,
            savesViewBase.SetInteractable,
            savesViewBase.AnimatedElements);
        var characterCreationPanel = new MainMenuScreenTransitionView.PanelHandle(
            view.CharacterCreationView.Panel,
            _characterCreationPresenter.Show,
            _characterCreationPresenter.Hide,
            view.CharacterCreationView.SetInteractable,
            view.CharacterCreationView.AnimatedElements);
        var transitionView = new MainMenuScreenTransitionView(
            view.Layout,
            menuPanel,
            settingsPanel,
            exitPanel,
            savesPanel,
            characterCreationPanel);
        var transitionAnimator = new MainMenuEdgeSlideTransitionAnimator();
        _transitionPresenter = new MainMenuScreenTransitionPresenter(
            transitionView,
            new MainMenuScreenTransitionModel(),
            transitionAnimator);
    }

    public void ApplyScreenLayoutImmediate(MainMenuScreen screen)
    {
        _transitionPresenter.ApplyScreenLayoutImmediate(screen);
    }

    public async UniTask<bool> TryTransitionToScreenAsync(MainMenuScreen targetScreen, CancellationToken token)
    {
        return await _transitionPresenter.TryTransitionToScreenAsync(targetScreen, token);
    }

    protected override void OnInitialize()
    {
        SubscribeOnEvents();

        _settingsPopupPresenter.Initialize();
        _exitPopupPresenter.Initialize();
        _savesPresenter.Initialize();
        _characterCreationPresenter.Initialize();

        view.SetVisible(model.IsVisible);
    }

    protected override async ValueTask OnInitializeAsync(CancellationToken token)
    {
        SubscribeOnEvents();

        await _settingsPopupPresenter.InitializeAsync(token);
        await _exitPopupPresenter.InitializeAsync(token);
        await _savesPresenter.InitializeAsync(token);
        await _characterCreationPresenter.InitializeAsync(token);

        view.SetVisible(model.IsVisible);
    }

    protected override void OnDispose()
    {
        UnsubscribeOneEvents();

        _settingsPopupPresenter.Dispose();
        _exitPopupPresenter.Dispose();
        _savesPresenter.Dispose();
        _characterCreationPresenter.Dispose();
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        UnsubscribeOneEvents();

        _settingsPopupPresenter.Dispose();
        _exitPopupPresenter.Dispose();
        _savesPresenter.Dispose();
        _characterCreationPresenter.Dispose();

        return default;
    }

    public override void Show()
    {
        model.Show();
        view.SetVisible(model.IsVisible);
    }

    public override void Hide()
    {
        model.Hide();
        view.SetVisible(model.IsVisible);
    }

    public override UniTask RequestPlayAsync()
    {
        return model.RequestPlayAsync();
    }

    public override UniTask RequestSettingsAsync()
    {
        return model.RequestSettingsAsync();
    }

    public override UniTask RequestExitAsync()
    {
        return model.RequestExitAsync();
    }

    public void PrepareCharacterCreation(int slotIndex)
    {
        _characterCreationPresenter.PrepareSlot(slotIndex);
    }

    private UniTask HandlePlayClicked()
    {
        return model.RequestPlayAsync();
    }

    private UniTask HandleSettingsClicked()
    {
        return model.RequestSettingsAsync();
    }

    private UniTask HandleExitClicked()
    {
        return model.RequestExitAsync();
    }

    private UniTask HandlePlayRequested()
    {
        return NotifyPlayRequestedAsync();
    }

    private UniTask HandleSettingsRequested()
    {
        return NotifySettingsRequestedAsync();
    }

    private UniTask HandleExitRequested()
    {
        return NotifyExitRequestedAsync();
    }

    private UniTask HandleSettingsBackRequested()
    {
        return SettingsBackRequested.InvokeAsync();
    }

    private UniTask HandleSettingsApplyRequested()
    {
        return SettingsApplyRequested.InvokeAsync();
    }

    private UniTask HandleExitConfirmed()
    {
        return ExitConfirmed.InvokeAsync();
    }

    private UniTask HandleExitCanceled()
    {
        return ExitCanceled.InvokeAsync();
    }

    private UniTask HandleSavesBackRequested()
    {
        return SavesBackRequested.InvokeAsync();
    }

    private UniTask HandleSaveSlotSelected(GameProfilesSlotData slot)
    {
        return SaveSlotSelected.InvokeAsync(slot);
    }

    private UniTask HandleCharacterCreationBackRequested()
    {
        return CharacterCreationBackRequested.InvokeAsync();
    }

    private UniTask HandleCharacterCreationCreateRequested(CharacterCreationResult result)
    {
        return CharacterCreationCreateRequested.InvokeAsync(result);
    }

    private void SubscribeOnEvents()
    {
        view.PlayClicked.Subscribe(HandlePlayClicked);
        view.SettingsClicked.Subscribe(HandleSettingsClicked);
        view.ExitClicked.Subscribe(HandleExitClicked);

        model.PlayRequested.Subscribe(HandlePlayRequested);
        model.SettingsRequested.Subscribe(HandleSettingsRequested);
        model.ExitRequested.Subscribe(HandleExitRequested);

        _settingsPopupPresenter.BackRequested.Subscribe(HandleSettingsBackRequested);
        _settingsPopupPresenter.ApplyRequested.Subscribe(HandleSettingsApplyRequested);

        _exitPopupPresenter.Confirmed.Subscribe(HandleExitConfirmed);
        _exitPopupPresenter.Canceled.Subscribe(HandleExitCanceled);

        _savesPresenter.BackRequested.Subscribe(HandleSavesBackRequested);
        _savesPresenter.SlotSelected.Subscribe(HandleSaveSlotSelected);

        _characterCreationPresenter.BackRequested.Subscribe(HandleCharacterCreationBackRequested);
        _characterCreationPresenter.CreateRequested.Subscribe(HandleCharacterCreationCreateRequested);
    }

    private void UnsubscribeOneEvents()
    {
        view.PlayClicked.Unsubscribe(HandlePlayClicked);
        view.SettingsClicked.Unsubscribe(HandleSettingsClicked);
        view.ExitClicked.Unsubscribe(HandleExitClicked);

        model.PlayRequested.Unsubscribe(HandlePlayRequested);
        model.SettingsRequested.Unsubscribe(HandleSettingsRequested);
        model.ExitRequested.Unsubscribe(HandleExitRequested);

        _settingsPopupPresenter.BackRequested.Unsubscribe(HandleSettingsBackRequested);
        _settingsPopupPresenter.ApplyRequested.Unsubscribe(HandleSettingsApplyRequested);

        _exitPopupPresenter.Confirmed.Unsubscribe(HandleExitConfirmed);
        _exitPopupPresenter.Canceled.Unsubscribe(HandleExitCanceled);

        _savesPresenter.BackRequested.Unsubscribe(HandleSavesBackRequested);
        _savesPresenter.SlotSelected.Unsubscribe(HandleSaveSlotSelected);

        _characterCreationPresenter.BackRequested.Unsubscribe(HandleCharacterCreationBackRequested);
        _characterCreationPresenter.CreateRequested.Unsubscribe(HandleCharacterCreationCreateRequested);
    }

}
}