using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationPathView : ExplorationPathViewBase
{
    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private float _lineWidth = 0.12f;

    [SerializeField]
    private Color _lineColor = new(1f, 0.86f, 0.29f);

    protected override void OnInitialize()
    {
        EnsureLineRenderer();
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        EnsureLineRenderer();
        return default;
    }

    protected override void OnDispose()
    {
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        return default;
    }

    private void Awake()
    {
        EnsureLineRenderer();
        HideRoute();
    }

    private void OnValidate()
    {
        EnsureLineRenderer();
    }

    public override void ShowRoute(IReadOnlyList<Vector3> worldPoints)
    {
        EnsureLineRenderer();

        if (worldPoints == null || worldPoints.Count == 0)
        {
            HideRoute();
            return;
        }

        _lineRenderer.enabled = true;
        _lineRenderer.positionCount = worldPoints.Count;
        for (var i = 0; i < worldPoints.Count; i++)
        {
            _lineRenderer.SetPosition(i, worldPoints[i] + Vector3.up * 0.05f);
        }
    }

    public override void HideRoute()
    {
        EnsureLineRenderer();
        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;
    }

    private void EnsureLineRenderer()
    {
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        _lineRenderer.useWorldSpace = true;
        _lineRenderer.loop = false;
        _lineRenderer.alignment = LineAlignment.View;
        _lineRenderer.widthMultiplier = _lineWidth;
        _lineRenderer.numCapVertices = 6;
        _lineRenderer.numCornerVertices = 4;
        _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _lineRenderer.receiveShadows = false;
        _lineRenderer.startColor = _lineColor;
        _lineRenderer.endColor = _lineColor;

        if (_lineRenderer.sharedMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default") ??
                         Shader.Find("Universal Render Pipeline/Unlit") ??
                         Shader.Find("Standard");
            if (shader != null)
            {
                _lineRenderer.material = new Material(shader);
            }
        }
    }
}
}
