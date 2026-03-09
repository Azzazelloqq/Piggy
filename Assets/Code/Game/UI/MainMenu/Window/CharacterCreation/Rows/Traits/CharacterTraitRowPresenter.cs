using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class CharacterTraitRowPresenter : CharacterTraitRowPresenterBase
{
    public CharacterTraitRowPresenter(CharacterTraitRowViewBase view, CharacterTraitRowModelBase model)
        : base(view, model)
    {
    }

    public override AsyncEvent<CharacterTraitSelection> SelectionChanged { get; } = new();
    public override CharacterTraitRowData Data => model.Data;

    public override void SetData(CharacterTraitRowData data)
    {
        model.SetData(data);
        view.SetData(data);
    }

    public override void SetInteractable(bool isInteractable)
    {
        view.SetInteractable(isInteractable);
    }

    public override async UniTask RequestToggleAsync(bool isSelected)
    {
        await model.RequestToggleAsync(isSelected);
        await SelectionChanged.InvokeAsync(new CharacterTraitSelection(model.Data.TraitId, isSelected));
    }

    protected override void OnInitialize()
    {
        view.Toggled.Subscribe(HandleToggled);
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        view.Toggled.Subscribe(HandleToggled);
        return default;
    }

    protected override void OnDispose()
    {
        view.Toggled.Unsubscribe(HandleToggled);
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        view.Toggled.Unsubscribe(HandleToggled);
        return default;
    }

    private UniTask HandleToggled(bool isSelected)
    {
        return RequestToggleAsync(isSelected);
    }
}
}
