using Disposable;
using UnityEngine;

namespace Code.Game.Exploration.View
{
public class ExplorationLocationView : MonoBehaviourDisposable
{
    [SerializeField]
    private string _locationId;

    public string LocationId => _locationId;
}
}
