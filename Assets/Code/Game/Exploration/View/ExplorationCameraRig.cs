using UnityEngine;

namespace Code.Game.Exploration.View
{
public sealed class ExplorationCameraRig : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    public Camera Camera => _camera;

    private void Reset()
    {
        ResolveCamera();
    }

    private void Awake()
    {
        EnsureCamera();
    }

    private void OnValidate()
    {
        ResolveCamera();
    }

    private void ResolveCamera()
    {
        if (_camera == null)
        {
            _camera = GetComponentInChildren<Camera>();
        }
    }

    private void EnsureCamera()
    {
        ResolveCamera();
        if (_camera == null)
        {
            _camera = gameObject.AddComponent<Camera>();
            _camera.orthographic = true;
        }
    }
}
}
