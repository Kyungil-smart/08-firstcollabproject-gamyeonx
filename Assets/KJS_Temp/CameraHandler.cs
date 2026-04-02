using UnityEngine;

/// <summary>
/// 터치 입력에 따라 카메라 팬(이동)과 핀치 줌을 처리한다.
/// 드래그 후 손을 떼면 관성에 의해 서서히 감속한다.
/// TouchInputHandler의 이벤트를 구독하여 동작한다.
/// </summary>
public class CameraHandler : MonoBehaviour
{
    [Header("팬")]
    [SerializeField] private float _panSpeed = 0.01f;
    [SerializeField] private float _deceleration = 5f;

    [Header("줌")]
    [SerializeField] private float _zoomSpeed = 0.01f;
    [SerializeField] private float _minZoom = 2f;
    [SerializeField] private float _maxZoom = 15f;

    private Camera _mainCamera;
    private Vector3 _velocity;
    private bool _isDragging;
    private bool _isUserControlEnabled = true;

    /// <summary>외부 시스템(팝업, UI 등)에서 카메라 조작을 강제로 잠금/해제한다.</summary>
    public bool IsControllable { get; set; } = true;

    private bool CanControl => IsControllable && _isUserControlEnabled;

    // -----------------------------------------------------
    private void Awake() => Init();
    private void OnEnable() => BindEvents();
    private void OnDisable() => UnbindEvents();
    // -----------------------------------------------------

    private void Init()
    {
        _mainCamera = Camera.main;
    }

    private void BindEvents()
    {
        TouchInputHandler input = TouchInputHandler.Instance;
        if (input == null) return;

        input.OnDragDelta += HandlePan;
        input.OnPinchDelta += HandleZoom;
        input.OnDragStarted += HandleDragStarted;
        input.OnDragEnded += HandleDragEnded;
        input.OnDoubleTouchTap += HandleDoubleTouchTap;
    }

    private void UnbindEvents()
    {
        TouchInputHandler input = TouchInputHandler.Instance;
        if (input == null) return;

        input.OnDragDelta -= HandlePan;
        input.OnPinchDelta -= HandleZoom;
        input.OnDragStarted -= HandleDragStarted;
        input.OnDragEnded -= HandleDragEnded;
        input.OnDoubleTouchTap -= HandleDoubleTouchTap;
    }

    private void Update()
    {
        if (_isDragging) return;
        if (_velocity.sqrMagnitude < 0.0001f) return;

        // 관성 이동
        _mainCamera.transform.position += _velocity * Time.deltaTime;

        // 감속
        _velocity = Vector3.Lerp(_velocity, Vector3.zero, _deceleration * Time.deltaTime);
    }

    private void HandleDragStarted()
    {
        if (!CanControl) return;

        _isDragging = true;
        _velocity = Vector3.zero;
    }

    private void HandleDragEnded(Vector2 lastDelta)
    {
        if (!CanControl)
        {
            _isDragging = false;
            return;
        }

        _isDragging = false;
        _velocity = new Vector3(-lastDelta.x, -lastDelta.y, 0f) * (_panSpeed * _mainCamera.orthographicSize);
    }

    private void HandleDoubleTouchTap()
    {
        _isUserControlEnabled = !_isUserControlEnabled;
        _velocity = Vector3.zero;
    }

    private void HandlePan(Vector2 delta)
    {
        if (!CanControl) return;

        Vector3 move = new Vector3(-delta.x, -delta.y, 0f) * (_panSpeed * _mainCamera.orthographicSize);
        _mainCamera.transform.position += move;
    }

    private void HandleZoom(float pinchDelta)
    {
        if (!CanControl) return;

        float newSize = _mainCamera.orthographicSize - pinchDelta * _zoomSpeed;
        _mainCamera.orthographicSize = Mathf.Clamp(newSize, _minZoom, _maxZoom);
    }
}
