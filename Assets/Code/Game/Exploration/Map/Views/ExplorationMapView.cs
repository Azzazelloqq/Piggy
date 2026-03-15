using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Exploration.Authoring;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationMapView : ExplorationMapViewBase
{
    [SerializeField]
    private LocationAuthoring _locationAuthoring;

    [SerializeField]
    private ExplorationPointView[] _pointViews;

    [SerializeField]
    private ExplorationActorView _actorView;

    [SerializeField]
    private ExplorationPathView _pathView;

    [SerializeField]
    private float _movementSpeed = 2.5f;

    public override LocationAuthoring LocationAuthoring => _locationAuthoring;
    public override IReadOnlyList<ExplorationPointView> PointViews => _pointViews ?? Array.Empty<ExplorationPointView>();
    public override ExplorationActorView ActorView => _actorView;
    public override ExplorationPathView PathView => _pathView;
    public override float MovementSpeed => Mathf.Max(0.1f, _movementSpeed);

    protected override void OnInitialize()
    {
        CollectChildren();
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        CollectChildren();
        return default;
    }

    protected override void OnDispose()
    {
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        return default;
    }

    private void OnValidate()
    {
        CollectChildren();
    }

    [ContextMenu("Collect Children")]
    private void CollectChildren()
    {
        if (_locationAuthoring == null)
        {
            _locationAuthoring = GetComponentInParent<LocationAuthoring>();
        }

        if (_locationAuthoring != null)
        {
            _pointViews = _locationAuthoring.GetComponentsInChildren<ExplorationPointView>(true);
        }

        if (_actorView == null)
        {
            _actorView = GetComponentInChildren<ExplorationActorView>(true);
        }

        if (_pathView == null)
        {
            _pathView = GetComponentInChildren<ExplorationPathView>(true);
        }
    }
}
}
