using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationMapModel : ExplorationMapModelBase
{
    private readonly Dictionary<string, ExplorationMapNodeData> _nodes;
    private readonly Dictionary<string, ExplorationMapPointData> _points;
    private string _currentNodeId;
    private Vector3 _currentWorldPosition;
    private string _selectedPointId;
    private bool _isMoving;

    public ExplorationMapModel(ExplorationMapDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        _nodes = new Dictionary<string, ExplorationMapNodeData>(StringComparer.Ordinal);
        foreach (var node in definition.Nodes ?? Array.Empty<ExplorationMapNodeData>())
        {
            if (string.IsNullOrWhiteSpace(node.NodeId))
            {
                continue;
            }

            _nodes[node.NodeId] = node;
        }

        _points = new Dictionary<string, ExplorationMapPointData>(StringComparer.Ordinal);
        foreach (var point in definition.Points ?? Array.Empty<ExplorationMapPointData>())
        {
            if (string.IsNullOrWhiteSpace(point.EntityId))
            {
                continue;
            }

            _points[point.EntityId] = point;
        }

        _currentNodeId = definition.CurrentNodeId;
        _currentWorldPosition = definition.StartWorldPosition;
        _selectedPointId = string.Empty;
    }

    public override IReadOnlyDictionary<string, ExplorationMapPointData> Points => _points;
    public override string CurrentNodeId => _currentNodeId;
    public override Vector3 CurrentWorldPosition => _currentWorldPosition;
    public override string SelectedPointId => _selectedPointId;
    public override bool IsMoving => _isMoving;

    public override bool TryPlanMovement(string pointId, out ExplorationMapRoutePlan routePlan)
    {
        routePlan = default;
        if (_isMoving || string.IsNullOrWhiteSpace(pointId))
        {
            return false;
        }

        if (!_points.TryGetValue(pointId, out var point) || !point.IsVisible || !point.IsInteractable)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(point.NodeId) || !_nodes.ContainsKey(point.NodeId))
        {
            return false;
        }

        if (!TryBuildNodeRoute(_currentNodeId, point.NodeId, out var routeNodeIds))
        {
            return false;
        }

        var waypoints = new List<Vector3>(routeNodeIds.Count + 2);
        AppendWaypoint(waypoints, _currentWorldPosition);

        if (_nodes.TryGetValue(_currentNodeId, out var currentNode))
        {
            AppendWaypoint(waypoints, currentNode.WorldPosition);
        }

        foreach (var routeNodeId in routeNodeIds)
        {
            if (_nodes.TryGetValue(routeNodeId, out var routeNode))
            {
                AppendWaypoint(waypoints, routeNode.WorldPosition);
            }
        }

        AppendWaypoint(waypoints, point.WorldPosition);
        _selectedPointId = pointId;

        var requiresMovement = CalculateRouteDistance(waypoints) > 0.01f;
        routePlan = new ExplorationMapRoutePlan(pointId, point.NodeId, waypoints.ToArray(), requiresMovement);
        return true;
    }

    public override void BeginMovement()
    {
        _isMoving = true;
    }

    public override void CancelMovement()
    {
        _isMoving = false;
    }

    public override void CompleteMovement(string pointId)
    {
        if (!_points.TryGetValue(pointId, out var point))
        {
            throw new InvalidOperationException($"Point '{pointId}' is not registered in the map model.");
        }

        _selectedPointId = pointId;
        _currentNodeId = point.NodeId;
        _currentWorldPosition = point.WorldPosition;
        _isMoving = false;
    }

    protected override void OnInitialize()
    {
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        return default;
    }

    protected override void OnDispose()
    {
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        return default;
    }

    private bool TryBuildNodeRoute(string startNodeId, string targetNodeId, out List<string> routeNodeIds)
    {
        routeNodeIds = new List<string>();
        if (string.IsNullOrWhiteSpace(startNodeId) || string.IsNullOrWhiteSpace(targetNodeId))
        {
            return false;
        }

        if (string.Equals(startNodeId, targetNodeId, StringComparison.Ordinal))
        {
            routeNodeIds.Add(startNodeId);
            return true;
        }

        var frontier = new Queue<string>();
        var previous = new Dictionary<string, string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        frontier.Enqueue(startNodeId);
        visited.Add(startNodeId);

        while (frontier.Count > 0)
        {
            var currentNodeId = frontier.Dequeue();
            if (!_nodes.TryGetValue(currentNodeId, out var currentNode))
            {
                continue;
            }

            foreach (var connectionId in currentNode.Connections ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(connectionId) || !_nodes.ContainsKey(connectionId) || !visited.Add(connectionId))
                {
                    continue;
                }

                previous[connectionId] = currentNodeId;
                if (string.Equals(connectionId, targetNodeId, StringComparison.Ordinal))
                {
                    routeNodeIds = ReconstructRoute(previous, startNodeId, targetNodeId);
                    return true;
                }

                frontier.Enqueue(connectionId);
            }
        }

        return false;
    }

    private static List<string> ReconstructRoute(
        Dictionary<string, string> previous,
        string startNodeId,
        string targetNodeId)
    {
        var route = new List<string>();
        var currentNodeId = targetNodeId;
        route.Add(currentNodeId);

        while (previous.TryGetValue(currentNodeId, out var previousNodeId))
        {
            currentNodeId = previousNodeId;
            route.Add(currentNodeId);
            if (string.Equals(currentNodeId, startNodeId, StringComparison.Ordinal))
            {
                break;
            }
        }

        route.Reverse();
        return route;
    }

    private static void AppendWaypoint(List<Vector3> waypoints, Vector3 point)
    {
        if (waypoints.Count == 0)
        {
            waypoints.Add(point);
            return;
        }

        if ((waypoints[^1] - point).sqrMagnitude > 0.0001f)
        {
            waypoints.Add(point);
        }
    }

    private static float CalculateRouteDistance(IReadOnlyList<Vector3> waypoints)
    {
        if (waypoints == null || waypoints.Count <= 1)
        {
            return 0f;
        }

        var totalDistance = 0f;
        for (var i = 1; i < waypoints.Count; i++)
        {
            totalDistance += Vector3.Distance(waypoints[i - 1], waypoints[i]);
        }

        return totalDistance;
    }
}
}
