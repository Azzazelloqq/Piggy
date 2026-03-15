using System;
using System.Threading;
using System.Threading.Tasks;
using Code.Game.Async;
using Code.Game.Exploration.Authoring;
using Code.Game.Exploration.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationPointView : ExplorationPointViewBase
{
    private const string PrototypeVisualName = "[PrototypeVisual]";

    [SerializeField]
    private string _entityId;

    [SerializeField]
    private PrimitiveType _primitiveType = PrimitiveType.Cylinder;

    [SerializeField]
    private Vector3 _visualOffset = new(0f, 0.1f, 0f);

    [SerializeField]
    private Vector3 _visualScale = new(0.55f, 0.15f, 0.55f);

    [SerializeField]
    private Vector3 _colliderSize = new(0.9f, 0.5f, 0.9f);

    [SerializeField]
    private Transform _visualRoot;

    [SerializeField]
    private Color _activityColor = new(0.94f, 0.54f, 0.18f);

    [SerializeField]
    private Color _transitionColor = new(0.23f, 0.63f, 0.94f);

    [SerializeField]
    private Color _disabledColor = new(0.35f, 0.35f, 0.35f);

    [SerializeField]
    private Color _selectedColor = new(0.98f, 0.9f, 0.47f);

    [SerializeField]
    private Color _hoverColor = new(1f, 0.96f, 0.82f);

    [SerializeField]
    private float _hoverScaleMultiplier = 1.05f;

    [SerializeField]
    private float _selectedScaleMultiplier = 1.08f;

    private readonly AsyncEvent _clicked = new();
    private BoxCollider _clickCollider;
    private SpriteRenderer _spriteRenderer;
    private Renderer _meshRenderer;
    private MapEntityType _entityType;
    private bool _isInteractable = true;
    private bool _isSelected;
    private bool _isHovered;
    private Vector3 _baseVisualScale = Vector3.one;

    public override AsyncEvent Clicked => _clicked;
    public override string EntityId => _entityId;

    public override void SetDisplayName(string displayName)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            gameObject.name = displayName;
        }
    }

    public override void SetEntityType(MapEntityType entityType)
    {
        _entityType = entityType;
        ApplyVisualState();
    }

    public override void SetVisible(bool isVisible)
    {
        _isHovered = false;
        gameObject.SetActive(isVisible);
    }

    public override void SetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;
        if (_clickCollider != null)
        {
            _clickCollider.enabled = isInteractable;
        }

        ApplyVisualState();
    }

    public override void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
        ApplyVisualState();
    }

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
        ResolveEntityId();
        EnsurePrototypeSetup();
    }

    private void Update()
    {
        UpdatePointerInteraction();
    }

    private void EnsurePrototypeSetup()
    {
        ResolveEntityId();
        EnsureVisual();
        EnsureCollider();
        ApplyVisualState();
    }

    private void ResolveEntityId()
    {
        if (!string.IsNullOrWhiteSpace(_entityId))
        {
            return;
        }

        var authoring = GetComponent<MapEntityAuthoring>();
        if (authoring != null)
        {
            _entityId = authoring.EntityId;
        }
    }

    private void EnsureCollider()
    {
        if (_clickCollider == null)
        {
            _clickCollider = GetComponent<BoxCollider>();
        }

        if (_clickCollider == null)
        {
            _clickCollider = gameObject.AddComponent<BoxCollider>();
        }

        if (_spriteRenderer != null && _spriteRenderer.sprite != null)
        {
            var spriteBounds = _spriteRenderer.sprite.bounds;
            var spriteCenter = Vector3.Scale(spriteBounds.center, _visualRoot.localScale);
            var spriteSize = Vector3.Scale(spriteBounds.size, _visualRoot.localScale);
            spriteSize.z = Mathf.Max(0.2f, _colliderSize.z);
            _clickCollider.center = _visualRoot.localPosition + spriteCenter;
            _clickCollider.size = spriteSize;
        }
        else
        {
            _clickCollider.center = new Vector3(_visualOffset.x, _colliderSize.y * 0.5f, _visualOffset.z);
            _clickCollider.size = _colliderSize;
        }

        _clickCollider.enabled = _isInteractable;
    }

    private void EnsureVisual()
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
    }

    private void ApplyVisualState()
    {
        if (_visualRoot == null)
        {
            return;
        }

        _visualRoot.localScale = _baseVisualScale * ResolveScaleMultiplier();

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = ResolveColor();
            return;
        }

        if (_meshRenderer == null)
        {
            return;
        }

        var targetMaterial = Application.isPlaying ? _meshRenderer.material : _meshRenderer.sharedMaterial;
        if (targetMaterial != null)
        {
            targetMaterial.color = ResolveColor();
        }
    }

    private Color ResolveColor()
    {
        if (!_isInteractable)
        {
            return _disabledColor;
        }

        if (_isSelected)
        {
            return _selectedColor;
        }

        if (_isHovered)
        {
            return _hoverColor;
        }

        if (_spriteRenderer != null)
        {
            return Color.white;
        }

        return _entityType == MapEntityType.Transition ? _transitionColor : _activityColor;
    }

    private float ResolveScaleMultiplier()
    {
        if (_isSelected)
        {
            return _selectedScaleMultiplier;
        }

        if (_isHovered)
        {
            return _hoverScaleMultiplier;
        }

        return 1f;
    }

    private void UpdatePointerInteraction()
    {
        if (_clickCollider == null)
        {
            return;
        }

        if (!TryGetPointerState(out var pointerScreenPosition, out var wasPressedThisFrame))
        {
            SetHovered(false);
            return;
        }

        var isHoveredNow = IsPointerOverCollider(pointerScreenPosition);
        SetHovered(isHoveredNow);

        if (isHoveredNow && _isInteractable && wasPressedThisFrame)
        {
            _clicked.InvokeAsync().Forget();
        }
    }

    private void SetHovered(bool isHovered)
    {
        if (_isHovered == isHovered)
        {
            return;
        }

        _isHovered = isHovered;
        ApplyVisualState();
    }

    private bool IsPointerOverCollider(Vector2 pointerScreenPosition)
    {
        if (_clickCollider == null)
        {
            return false;
        }

        var targetCamera = ResolveTargetCamera();
        if (targetCamera == null)
        {
            return false;
        }

        var pointerRay = targetCamera.ScreenPointToRay(pointerScreenPosition);
        return Physics.Raycast(pointerRay, out var hitInfo, float.MaxValue, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) &&
               hitInfo.collider == _clickCollider;
    }

    private static Camera ResolveTargetCamera()
    {
        if (Camera.main != null)
        {
            return Camera.main;
        }

        return Camera.current ?? Object.FindFirstObjectByType<Camera>();
    }

    private static bool TryGetPointerState(out Vector2 pointerScreenPosition, out bool wasPressedThisFrame)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            pointerScreenPosition = Mouse.current.position.ReadValue();
            wasPressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        pointerScreenPosition = Input.mousePosition;
        wasPressedThisFrame = Input.GetMouseButtonDown(0);
        return true;
#else
        pointerScreenPosition = default;
        wasPressedThisFrame = false;
        return false;
#endif
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
