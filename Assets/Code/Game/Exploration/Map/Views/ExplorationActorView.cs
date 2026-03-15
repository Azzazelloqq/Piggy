using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationActorView : ExplorationActorViewBase
{
    private const string PrototypeVisualName = "[ActorPrototype]";

    [SerializeField]
    private PrimitiveType _primitiveType = PrimitiveType.Capsule;

    [SerializeField]
    private Vector3 _visualOffset = new(0f, 0.45f, 0f);

    [SerializeField]
    private Vector3 _visualScale = new(0.4f, 0.9f, 0.4f);

    [SerializeField]
    private Transform _visualRoot;

    [SerializeField]
    private Color _color = Color.white;

    private SpriteRenderer _spriteRenderer;
    private Renderer _meshRenderer;
    private Vector3 _baseVisualScale = Vector3.one;

    protected override void OnInitialize()
    {
        EnsurePrototypeSetup();
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        EnsurePrototypeSetup();
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
        EnsurePrototypeSetup();
    }

    private void OnValidate()
    {
        EnsurePrototypeSetup();
    }

    public override void SetWorldPosition(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }

    public override async UniTask MoveAlongAsync(IReadOnlyList<Vector3> waypoints, float speed, CancellationToken token)
    {
        EnsurePrototypeSetup();

        if (waypoints == null || waypoints.Count == 0)
        {
            return;
        }

        transform.position = waypoints[0];
        if (speed <= 0f)
        {
            transform.position = waypoints[^1];
            return;
        }

        for (var i = 1; i < waypoints.Count; i++)
        {
            var target = waypoints[i];
            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                token.ThrowIfCancellationRequested();
                var step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, target, step);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            transform.position = target;
        }
    }

    private void EnsurePrototypeSetup()
    {
        var createdVisual = false;
        if (_visualRoot == null)
        {
            var existing = transform.Find(PrototypeVisualName);
            if (existing != null)
            {
                _visualRoot = existing;
            }
        }

        if (_visualRoot == null)
        {
            var primitive = GameObject.CreatePrimitive(_primitiveType);
            primitive.name = PrototypeVisualName;
            primitive.transform.SetParent(transform, false);
            _visualRoot = primitive.transform;
            createdVisual = true;

            var primitiveCollider = primitive.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                DestroyComponent(primitiveCollider);
            }
        }

        _spriteRenderer = _visualRoot.GetComponent<SpriteRenderer>();
        _meshRenderer = _visualRoot.GetComponent<Renderer>();

        if (createdVisual || _spriteRenderer == null)
        {
            _visualRoot.localPosition = _visualOffset;
            _visualRoot.localRotation = Quaternion.identity;
            _visualRoot.localScale = _visualScale;
        }

        _baseVisualScale = _visualRoot.localScale;
        _visualRoot.localScale = _baseVisualScale;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _color;
            return;
        }

        if (_meshRenderer == null)
        {
            return;
        }

        var targetMaterial = Application.isPlaying ? _meshRenderer.material : _meshRenderer.sharedMaterial;
        if (targetMaterial != null)
        {
            targetMaterial.color = _color;
        }
    }

    private static void DestroyComponent(Component component)
    {
        if (component == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Object.DestroyImmediate(component);
            return;
        }
#endif
        Object.Destroy(component);
    }
}
}
