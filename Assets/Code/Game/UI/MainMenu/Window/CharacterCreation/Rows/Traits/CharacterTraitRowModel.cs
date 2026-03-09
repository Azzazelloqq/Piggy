using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class CharacterTraitRowModel : CharacterTraitRowModelBase
{
    private CharacterTraitRowData _data;

    public override CharacterTraitRowData Data => _data;

    public override void SetData(CharacterTraitRowData data)
    {
        _data = data;
    }

    public override UniTask RequestToggleAsync(bool isSelected)
    {
        _data = new CharacterTraitRowData(
            _data.TraitId,
            _data.LocalizationKey,
            _data.FallbackLabel,
            isSelected,
            _data.IsInteractable);
        return UniTask.CompletedTask;
    }

    protected override void OnInitialize()
    {
        _data = default;
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        _data = default;
        return default;
    }

    protected override void OnDispose()
    {
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        return default;
    }
}
}
