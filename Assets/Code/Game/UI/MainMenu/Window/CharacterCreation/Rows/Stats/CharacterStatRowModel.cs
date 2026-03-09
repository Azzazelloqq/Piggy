using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class CharacterStatRowModel : CharacterStatRowModelBase
{
    private CharacterStatRowData _data;

    public override CharacterStatRowData Data => _data;

    public override void SetData(CharacterStatRowData data)
    {
        _data = data;
    }

    public override UniTask RequestIncreaseAsync()
    {
        return UniTask.CompletedTask;
    }

    public override UniTask RequestDecreaseAsync()
    {
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
