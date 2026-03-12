using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Game.Exploration.Authoring
{
public sealed class MapEntityAuthoring : MonoBehaviour
{
    [SerializeField]
    private string _entityId;

    [SerializeField]
    private MapEntityType _type;

    [SerializeField]
    private MapNodeAuthoring _node;

    [SerializeField]
    private ActivityViewAuthoring _activityView;

    [SerializeField]
    private TransitionAuthoring _transition;

    public string EntityId => _entityId;
    public MapEntityType Type => _type;
    public MapNodeAuthoring Node => _node;
    public ActivityViewAuthoring ActivityView => _activityView;
    public TransitionAuthoring Transition => _transition;
}
}
