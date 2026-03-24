using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Pan Settings")] 
    public float PanSpeed = 1f;
    [Range(0f, 1f)] 
    public float Reducing = 0.92f;

    [Header("Zoom Settings")] 
    public float ZoomSpeed = 0.05f;
    public float MaxSize = 10f;
    public float MinSize = 3f;

    [Header("Bounds (유효 타일 영역)")] 
    public Bounds CameraBounds;

    [Header("이동 설정")]
    public float MoveDuration = 0.5f;

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

    // 카메라 이동
    private bool _isMoving;
    private bool _isReturning;
    private Vector3 _moveStartPos;
    private Vector3 _moveTargetPos;
    private float _moveElapsed;

    // 내부건물 이동 전 카메라 정보
    private Bounds _prevBounds;
    private float _prevMinSize;
    private float _prevMaxSize;
    private Vector3 _prevCameraPos;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
    }

    void Update()
    {
        if (_isMoving)
        {
            HandleCameraMove();
            return;
        }

        if (IsInputLocked) return;

#if UNITY_EDITOR
        HandleMouseInput();
#else
        int touchCount = Input.touchCount;
        if (touchCount >= 2)      HandlePinchZoom();
        else if (touchCount == 1) HandlePan();
        else                      ApplyInertia();
#endif

        ClampPosition();
    }

    // ───────────────────────────────
    // 단일 터치 Pan + 건물 탭 감지
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

            _isPanning = false;
            _isPinching = false;
            _touchStartedOnBuilding = false;
        }
    }

    // ───────────────────────────────
    // 관성 감속
    // ───────────────────────────────
    void ApplyInertia()
    {
        if (_velocity.sqrMagnitude < 0.001f)
        {
            _velocity = Vector3.zero;
            return;
        }
        transform.position += _velocity * Time.deltaTime;
        _velocity *= Reducing;
    }

    // ───────────────────────────────
    // 두 손가락 Pinch Zoom
    // ───────────────────────────────
    void HandlePinchZoom()
    {
        _velocity = Vector3.zero;
        _isPanning = false;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        if (IsTouchOverUI(t0.fingerId) || IsTouchOverUI(t1.fingerId)) return;

        float currentDist = Vector2.Distance(t0.position, t1.position);

        if (!_isPinching)
        {
            _prevPinchDist = currentDist;
            _isPinching = true;
            return;
        }

        float delta = _prevPinchDist - currentDist;
        _cam.orthographicSize = Mathf.Clamp(
            _cam.orthographicSize + delta * ZoomSpeed,
            MinSize, MaxSize
        );
        _prevPinchDist = currentDist;
    }

    // ───────────────────────────────
    // 카메라 이동 (Bounds + Zoom 동시 변경)
    // ───────────────────────────────
    public void MoveToBuilding(Transform pivot, Vector2 boundsSize, float minSize, float maxSize)
    {
        if (_isMoving || IsInputLocked) return;
        
        _prevBounds = CameraBounds;
        _prevMinSize = MinSize;
        _prevMaxSize = MaxSize;
        _prevCameraPos = transform.position;
        
        CameraBounds = new Bounds(pivot.position, new Vector3(boundsSize.x, boundsSize.y, 0));
        MinSize = minSize;
        MaxSize = maxSize;

        _moveStartPos = transform.position;
        _moveTargetPos = new Vector3(pivot.position.x, pivot.position.y, transform.position.z);
        _moveElapsed = 0f;
        _isMoving = true;
        _isReturning = false;
        _velocity = Vector3.zero;
        IsInputLocked = true;
    }

    // ───────────────────────────────
    // 외부 맵으로 복귀
    // ───────────────────────────────
    public void ReturnToWorld()
    {
        // Bounds를 크게 설정해 이동 중 ClampPosition에 막히지 않도록
        CameraBounds = new Bounds(Vector3.zero, new Vector3(9999, 9999, 0));

        _moveStartPos = transform.position;
        _moveTargetPos = _prevCameraPos;
        _moveElapsed = 0f;
        _isMoving = true;
        _isReturning = true;
        _velocity = Vector3.zero;
        IsInputLocked = true;
    }

    // ───────────────────────────────
    // 카메라 이동 처리 (Update에서 호출)
    // 코루틴 대신 Update로 처리 - 이동 중 다른 건물 내부 보이는 현상 방지
    // ───────────────────────────────
    void HandleCameraMove()
    {
        _moveElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_moveElapsed / MoveDuration);
        t = t * t * (3f - 2f * t); // SmoothStep

        // 보간된 위치 계산
        Vector3 nextPos = Vector3.Lerp(_moveStartPos, _moveTargetPos, t);

        // [수정] 복귀 중에는 절대 Clamp하지 않음 (Bounds 무시하고 강제 이동)
        if (!_isReturning)
        {
            nextPos.x = Mathf.Clamp(nextPos.x, CameraBounds.min.x, CameraBounds.max.x);
            nextPos.y = Mathf.Clamp(nextPos.y, CameraBounds.min.y, CameraBounds.max.y);
        }

        transform.position = nextPos;

        if (_moveElapsed >= MoveDuration)
        {
            // 이동 완료 시점
            transform.position = _moveTargetPos;
            _isMoving = false;
        
            if (_isReturning)
            {
                // [핵심] 복귀가 끝난 '후'에야 Bounds를 복구함
                CameraBounds = _prevBounds;
                MinSize = _prevMinSize;
                MaxSize = _prevMaxSize;
                _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, MinSize, MaxSize);
            
                _isReturning = false; // 플래그 해제
            }

            IsInputLocked = false;
            _touchStartedOnBuilding = false;
            _mouseDownOnUI = false;
        }
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
    // UI 터치 여부 체크
    // ───────────────────────────────
    bool IsTouchOverUI(int fingerId)
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    // ───────────────────────────────
    // 팝업 시스템 연동용
    // ───────────────────────────────
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

    // ───────────────────────────────
    // 에디터 전용 마우스 조작
    // ───────────────────────────────
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _lastPanPos = Input.mousePosition;
            _touchBeganPos = Input.mousePosition;
            _velocity = Vector3.zero;
            _isPanning = false;

            // UI 위 클릭이면 건물 감지 스킵
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
            if (Vector2.Distance(
                new Vector2(Input.mousePosition.x, Input.mousePosition.y), _touchBeganPos) > DragThreshold)
            {
                _isPanning = true;
                _touchStartedOnBuilding = false;
            }

            if (_isPanning)
            {
                Vector3 prevWorld = _cam.ScreenToWorldPoint(new Vector3(_lastPanPos.x, _lastPanPos.y, 0));
                Vector3 currWorld = _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

                Vector3 delta = (prevWorld - currWorld) * PanSpeed;
                delta.z = 0;

                _velocity = delta / Time.deltaTime;
                transform.position += delta;
                _lastPanPos = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // UI에서 시작된 클릭이면 건물 탭 무시
            if (!_mouseDownOnUI && !_isPanning && _touchStartedOnBuilding)
            {
                Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                hit.collider?.GetComponent<BuildingData>()?.CanvasActive();
            }

            _isPanning = false;
            _touchStartedOnBuilding = false;
            _mouseDownOnUI = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            _cam.orthographicSize = Mathf.Clamp(
                _cam.orthographicSize - scroll * 5f,
                MinSize, MaxSize
            );
        }

        if (!Input.GetMouseButton(0))
            ApplyInertia();
    }
}