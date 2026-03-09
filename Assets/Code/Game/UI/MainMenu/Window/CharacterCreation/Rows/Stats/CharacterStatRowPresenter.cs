using System.Threading;
using System.Threading.Tasks;
using Code.Config.Pages.CharactersPage;
using Code.Game.Async;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class CharacterStatRowPresenter : CharacterStatRowPresenterBase
{
    public CharacterStatRowPresenter(CharacterStatRowViewBase view, CharacterStatRowModelBase model)
        : base(view, model)
    {
    }

    public override AsyncEvent<CharacterStatType> IncreaseRequested { get; } = new();
    public override AsyncEvent<CharacterStatType> DecreaseRequested { get; } = new();
    public override CharacterStatRowData Data => model.Data;

    public override void SetData(CharacterStatRowData data)
    {
        model.SetData(data);
        view.SetData(data);
    }

    public override void SetInteractable(bool isInteractable)
    {
        view.SetInteractable(isInteractable);
    }

    public override async UniTask RequestIncreaseAsync()
    {
        await model.RequestIncreaseAsync();
        await IncreaseRequested.InvokeAsync(model.Data.StatType);
    }

    public override async UniTask RequestDecreaseAsync()
    {
        await model.RequestDecreaseAsync();
        await DecreaseRequested.InvokeAsync(model.Data.StatType);
    }

    protected override void OnInitialize()
    {
        view.IncrementClicked.Subscribe(HandleIncrementClicked);
        view.DecrementClicked.Subscribe(HandleDecrementClicked);
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        view.IncrementClicked.Subscribe(HandleIncrementClicked);
        view.DecrementClicked.Subscribe(HandleDecrementClicked);
        return default;
    }

    protected override void OnDispose()
    {
        view.IncrementClicked.Unsubscribe(HandleIncrementClicked);
        view.DecrementClicked.Unsubscribe(HandleDecrementClicked);
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        view.IncrementClicked.Unsubscribe(HandleIncrementClicked);
        view.DecrementClicked.Unsubscribe(HandleDecrementClicked);
        return default;
    }

    private UniTask HandleIncrementClicked()
    {
        return RequestIncreaseAsync();
    }

    private UniTask HandleDecrementClicked()
    {
        return RequestDecreaseAsync();
    }
}
}
