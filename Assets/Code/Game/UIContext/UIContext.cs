using System;
using UnityEngine;

namespace Code.Game.Root
{
[Serializable]
public class UIContext : MonoBehaviour
{
    [field: SerializeField]
    public RectTransform OverlaysParent { get; private set; }
    
    [field: SerializeField]
    public RectTransform HUDParent { get; private set; }
    
    [field: SerializeField]
    public RectTransform DynamicObjectsParent { get; private set; }
}
}