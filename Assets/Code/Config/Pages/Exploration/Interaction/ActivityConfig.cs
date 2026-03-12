using System;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[Serializable]
public struct ActivityConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private ActivityOptionConfig[] _options;

    public string Id => _id;
    public string DisplayName => _displayName;
    public ActivityOptionConfig[] Options => _options;
}
}
