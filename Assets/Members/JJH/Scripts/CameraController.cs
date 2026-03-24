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
    public float MoveDuration = 0.5f;  // 카메라 이동 시간

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
    private Vector2 _touchBeganPos;        // 터치 시작 위치 (드래그 판단용)
    private const float DragThreshold = 10f; // 드래그로 판단할 최소 픽셀

    // 카메라 이동 (코루틴 대신 Update에서 처리)
    private bool _isMoving;
    private Vector3 _moveStartPos;
    private Vector3 _moveTargetPos;
    private float _moveElapsed;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
    }

    void Update()
    {
        // 카메라 이동 중이면 이동 처리 먼저
        if (_isMoving)
        {
            HandleCameraMove();
            return;  // 이동 중엔 터치 입력 전부 차단
        }

        if (IsInputLocked) return;

#if UNITY_EDITOR
        HandleMouseInput();
#else
        int touchCount = Input.touchCount;

        if (touchCount >= 2)
            HandlePinchZoom();
        else if (touchCount == 1)
            HandlePan();
        else
            ApplyInertia();
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
            _isPanning = false;  // 드래그 확정 전까지 false

            // 터치 시작 위치에 건물 있는지 체크
            Vector2 worldPos = _cam.ScreenToWorldPoint(touch.position);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            _touchStartedOnBuilding = hit.collider != null &&
                                      hit.collider.GetComponent<BuildingData>() != null;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            // 시작점에서 DragThreshold 이상 움직이면 드래그로 확정
            float dragDist = Vector2.Distance(touch.position, _touchBeganPos);
            if (dragDist > DragThreshold)
            {
                _isPanning = true;
                _touchStartedOnBuilding = false;  // 드래그면 건물 탭 무효
            }

            if (_isPanning)
            {
                Vector3 prevWorld = _cam.ScreenToWorldPoint(
                    new Vector3(_lastPanPos.x, _lastPanPos.y, 0));
                Vector3 currWorld = _cam.ScreenToWorldPoint(
                    new Vector3(touch.position.x, touch.position.y, 0));

                Vector3 delta = (prevWorld - currWorld) * PanSpeed;
                delta.z = 0;

                _velocity = delta / Time.deltaTime;
                transform.position += delta;
                _lastPanPos = touch.position;
            }
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            // 드래그 없이 뗐고 건물 위였으면 → 건물 탭 처리
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
        // 1. Bounds 먼저 변경 (ClampPosition이 목적지를 막지 않도록)
        CameraBounds = new Bounds(
            new Vector3(pivot.position.x, pivot.position.y, 0),  // Pivot이 새 Center
            new Vector3(boundsSize.x, boundsSize.y, 0)
        );

        // 2. 줌 범위 변경
        MinSize = minSize;
        MaxSize = maxSize;

        // 현재 줌이 새 범위 벗어나면 즉시 보정
        _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, MinSize, MaxSize);

        // 3. 이동 시작 세팅
        _moveStartPos = transform.position;
        _moveTargetPos = new Vector3(pivot.position.x, pivot.position.y, transform.position.z);
        _moveElapsed = 0f;
        _isMoving = true;
        _velocity = Vector3.zero;

        IsInputLocked = true;  // 이동 중 조작 차단
    }

    // ───────────────────────────────
    // 카메라 이동 처리 (Update에서 호출)
    // 코루틴 대신 Update로 처리 - 이동 중 다른 건물 내부 보이는 현상 방지
    // ───────────────────────────────
    void HandleCameraMove()
    {
        _moveElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_moveElapsed / MoveDuration);

        // EaseInOut : 처음엔 빠르게, 끝에서 천천히
        t = t * t * (3f - 2f * t);

        transform.position = Vector3.Lerp(_moveStartPos, _moveTargetPos, t);
        ClampPosition();

        // 이동 완료
        if (_moveElapsed >= MoveDuration)
        {
            transform.position = _moveTargetPos;
            _isMoving = false;
            IsInputLocked = false;  // 이동 완료 후 조작 재활성화
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

            // 건물 위 클릭인지 체크
            Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            _touchStartedOnBuilding = hit.collider != null &&
                                      hit.collider.GetComponent<BuildingData>() != null;
        }
        else if (Input.GetMouseButton(0))
        {
            float dragDist = Vector2.Distance(
                new Vector2(Input.mousePosition.x, Input.mousePosition.y), _touchBeganPos);

            if (dragDist > DragThreshold)
            {
                _isPanning = true;
                _touchStartedOnBuilding = false;
            }

            if (_isPanning)
            {
                Vector3 prevWorld = _cam.ScreenToWorldPoint(
                    new Vector3(_lastPanPos.x, _lastPanPos.y, 0));
                Vector3 currWorld = _cam.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

                Vector3 delta = (prevWorld - currWorld) * PanSpeed;
                delta.z = 0;

                _velocity = delta / Time.deltaTime;
                transform.position += delta;
                _lastPanPos = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 드래그 없이 뗐고 건물 위였으면 → 건물 클릭 처리
            if (!_isPanning && _touchStartedOnBuilding)
            {
                Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                hit.collider?.GetComponent<BuildingData>()?.CanvasActive();
            }

            _isPanning = false;
            _touchStartedOnBuilding = false;
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