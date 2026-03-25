using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Pan Settings")] 
    public float PanSpeed = 1f;
    [Range(0f, 1f)] public float Reducing = 0.92f;

    [Header("Zoom Settings")] 
    public float ZoomSpeed = 0.05f;
    public float MaxSize = 10f;
    public float MinSize = 3f;

    [Header("Bounds (유효 타일 영역)")] 
    public Bounds CameraBounds;

    [HideInInspector] 
    public bool IsInputLocked = false;

    private Camera _cam;
    private Vector3 _velocity;
    private bool _isPanning;
    private Vector2 _lastPanPos;

    // Pinch
    private float _prevPinchDist;
    private bool _isPinching;

    // 건물 탭 감지
    private bool _touchStartedOnBuilding;
    private bool _mouseDownOnUI;
    private Vector2 _touchBeganPos;
    private const float DragThreshold = 10f;

    // 내부건물 이동 전 카메라 정보 저장용
    private Bounds _prevBounds;
    private float _prevMinSize;
    private float _prevMaxSize;
    private Vector3 _prevCameraPos;
    private bool _isInBuilding = false; // 현재 건물 내부인지 체크하는 플래그

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        
        // 시작할 때의 바운즈를 초기값으로 설정
        _prevBounds = CameraBounds;
        _prevMinSize = MinSize;
        _prevMaxSize = MaxSize;
    }

    void Update()
    {
        if (!IsInputLocked)
        {
#if UNITY_EDITOR
            HandleMouseInput();
#else
            int touchCount = Input.touchCount;
            if (touchCount >= 2)      HandlePinchZoom();
            else if (touchCount == 1) HandlePan();
            else                      ApplyInertia();
#endif
        }

        ClampPosition();
    }

    // ───────────────────────────────
    // 카메라 이동 (건물 내부로 순간이동)
    // ───────────────────────────────
    public void MoveToBuilding(Transform pivot, Vector2 boundsSize, float minSize, float maxSize)
    {
        // 이미 건물 내부라면 이전 좌표(월드 좌표)를 다시 덮어씌우지 않음
        if (!_isInBuilding)
        {
            _prevBounds = CameraBounds;
            _prevMinSize = MinSize;
            _prevMaxSize = MaxSize;
            _prevCameraPos = transform.position;
            _isInBuilding = true;
        }

        // 새로운 내부 영역 설정
        CameraBounds = new Bounds(pivot.position, new Vector3(boundsSize.x, boundsSize.y, 0));
        MinSize = minSize;
        MaxSize = maxSize;
        _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, MinSize, MaxSize);

        // 즉시 이동
        transform.position = new Vector3(pivot.position.x, pivot.position.y, transform.position.z);
        
        _velocity = Vector3.zero;
        _isPanning = false;
        IsInputLocked = true; 

        ClampPosition();
    }

    // ───────────────────────────────
    // 외부 맵으로 복귀 (순간이동)
    // ───────────────────────────────
    public void ReturnToWorld()
    {
        // 저장된 월드 데이터로 강제 복구
        CameraBounds = _prevBounds;
        MinSize = _prevMinSize;
        MaxSize = _prevMaxSize;
        _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, MinSize, MaxSize);

        // 저장된 위치로 복귀
        transform.position = _prevCameraPos;

        // 상태 초기화
        _velocity = Vector3.zero;
        _isPanning = false;
        _isPinching = false;
        IsInputLocked = false;
        _isInBuilding = false; // 월드로 돌아왔음을 표시

        // 즉시 위치 강제 고정
        ClampPosition();
        
        Debug.Log($"World 복귀 완료! 목표 좌표: {_prevCameraPos}");
    }

    // ───────────────────────────────
    // 카메라 중심 경계 제한
    // ───────────────────────────────
    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, CameraBounds.min.x, CameraBounds.max.x);
        pos.y = Mathf.Clamp(pos.y, CameraBounds.min.y, CameraBounds.max.y);
        transform.position = pos;
    }

    // ───────────────────────────────
    // 입력 로직
    // ───────────────────────────────
    void HandlePan()
    {
        Touch touch = Input.GetTouch(0);
        if (IsTouchOverUI(touch.fingerId)) return;

        if (touch.phase == TouchPhase.Began)
        {
            _lastPanPos = touch.position;
            _touchBeganPos = touch.position;
            _velocity = Vector3.zero;
            _isPanning = false;
            Vector2 worldPos = _cam.ScreenToWorldPoint(touch.position);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            _touchStartedOnBuilding = hit.collider?.GetComponent<BuildingData>() != null;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            if (Vector2.Distance(touch.position, _touchBeganPos) > DragThreshold)
            {
                _isPanning = true;
                _touchStartedOnBuilding = false;
            }
            if (_isPanning)
            {
                Vector3 prevWorld = _cam.ScreenToWorldPoint(new Vector3(_lastPanPos.x, _lastPanPos.y, 0));
                Vector3 currWorld = _cam.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
                Vector3 delta = (prevWorld - currWorld) * PanSpeed;
                delta.z = 0;
                _velocity = delta / Time.deltaTime;
                transform.position += delta;
                _lastPanPos = touch.position;
            }
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            if (!_isPanning && _touchStartedOnBuilding)
            {
                Vector2 worldPos = _cam.ScreenToWorldPoint(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                hit.collider?.GetComponent<BuildingData>()?.CanvasActive();
            }
            _isPanning = _isPinching = _touchStartedOnBuilding = false;
        }
    }

    void ApplyInertia()
    {
        if (_isPanning) return;
        if (_velocity.sqrMagnitude < 0.001f) { _velocity = Vector3.zero; return; }
        transform.position += _velocity * Time.deltaTime;
        _velocity *= Reducing;
    }

    void HandlePinchZoom()
    {
        _velocity = Vector3.zero;
        _isPanning = false;
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);
        if (IsTouchOverUI(t0.fingerId) || IsTouchOverUI(t1.fingerId)) return;
        float currentDist = Vector2.Distance(t0.position, t1.position);
        if (!_isPinching) { _prevPinchDist = currentDist; _isPinching = true; return; }
        float delta = _prevPinchDist - currentDist;
        _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize + delta * ZoomSpeed, MinSize, MaxSize);
        _prevPinchDist = currentDist;
    }

    bool IsTouchOverUI(int fingerId) => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _lastPanPos = Input.mousePosition;
            _touchBeganPos = Input.mousePosition;
            _velocity = Vector3.zero;
            _isPanning = false;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                _mouseDownOnUI = true;
                _touchStartedOnBuilding = false;
            }
            else
            {
                _mouseDownOnUI = false;
                Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                _touchStartedOnBuilding = hit.collider?.GetComponent<BuildingData>() != null;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (Vector2.Distance(Input.mousePosition, _touchBeganPos) > DragThreshold)
            {
                _isPanning = true;
                _touchStartedOnBuilding = false;
            }
            if (_isPanning)
            {
                Vector3 prevWorld = _cam.ScreenToWorldPoint(_lastPanPos);
                Vector3 currWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 delta = (prevWorld - currWorld) * PanSpeed;
                delta.z = 0;
                _velocity = delta / Time.deltaTime;
                transform.position += delta;
                _lastPanPos = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!_mouseDownOnUI && !_isPanning && _touchStartedOnBuilding)
            {
                Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                hit.collider?.GetComponent<BuildingData>()?.CanvasActive();
            }
            _isPanning = _touchStartedOnBuilding = _mouseDownOnUI = false;
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f) _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize - scroll * 5f, MinSize, MaxSize);
        if (!Input.GetMouseButton(0)) ApplyInertia();
    }

    public void SetInputLock(bool locked)
    {
        IsInputLocked = locked;
        if (locked) _velocity = Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(CameraBounds.center, CameraBounds.size);
    }
}