using System;
using System.Collections.Generic;
using Azzazelloqq.Config;
using UnityEngine;

namespace Code.Config
{
[CreateAssetMenu(menuName = "Config/MainConfig", fileName = "MainConfig")]
public sealed class MainConfig : ScriptableObject
{
    [SerializeField]
    private ScriptableObject[] _pages;

    public IConfigPage[] GetPages()
    {
        var result = new List<IConfigPage>(_pages.Length);
        foreach (var page in _pages)
        {
            if (page == null)
            {
                continue;
            }

            if (page is IConfigPage configPage)
            {
                result.Add(configPage);
                continue;
            }

            throw new InvalidOperationException(
                "Config page does not implement IConfigPage: " + page.name);
        }

        return result.ToArray();
    }
}
}
