using UnityEngine;

namespace Code.Game.Exploration.Authoring
{
public sealed class ActivityViewAuthoring : MonoBehaviour
{
    [SerializeField]
    private string _activityId;

    [SerializeField]
    private GameObject _viewPrefab;

    public string ActivityId => _activityId;
    public GameObject ViewPrefab => _viewPrefab;
}
}
