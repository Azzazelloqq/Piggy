using System;
using Code.Game.Loading.Window;
using Disposable;
using UnityEngine;

namespace Code.Game.Root
{
[Serializable]
public struct UIContext
{
    [field: SerializeField]
    public RectTransform MainUIParent { get; private set; }
    
    [field: SerializeField]
    public RectTransform OverlaysParent { get; private set; }
    
    [field: SerializeField]
    public RectTransform HUDParent { get; private set; }
    
    [field: SerializeField]
    public RectTransform DynamicObjectsParent { get; private set; }

    [field: SerializeField]
    public LoadingWindowViewBase LoadingWindowPrefab { get; private set; }
}
}