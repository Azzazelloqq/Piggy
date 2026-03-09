using System;
using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.Config;

namespace Code.Config
{
public sealed class ScriptableObjectConfigParser : IConfigParser
{
    private readonly IConfigPage[] _pages;

    public ScriptableObjectConfigParser(MainConfig pagesAsset)
    {
        if (pagesAsset == null)
        {
            throw new ArgumentNullException(nameof(pagesAsset));
        }

        _pages = pagesAsset.GetPages();
    }

    public ScriptableObjectConfigParser(params IConfigPage[] pages)
    {
        _pages = pages ?? throw new ArgumentNullException(nameof(pages));
    }

    public IConfigPage[] Parse()
    {
        return _pages;
    }

    public Task<IConfigPage[]> ParseAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(_pages);
    }

    public Task<IConfigPage[]> ParseAsync(IProgress<ParseProgress> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        progress?.Report(new ParseProgress(
            1f,
            "ScriptableObjectConfigParser: parsed " + _pages.Length + " page(s)"));
        return Task.FromResult(_pages);
    }

    public void ParseAsync(Action<ParseProgress> progress, Action<IConfigPage[]> onParsed, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        progress?.Invoke(new ParseProgress(
            1f,
            "ScriptableObjectConfigParser: parsed " + _pages.Length + " page(s)"));
        onParsed?.Invoke(_pages);
    }
}
}
