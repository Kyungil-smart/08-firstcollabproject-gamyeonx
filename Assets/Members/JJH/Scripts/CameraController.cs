using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Pan Settings")] public float PanSpeed = 1f;
    [Range(0f, 1f)] public float Reducing = 0.92f; // 1에 가까울수록 천천히 멈춤

    [Header("Zoom Settings")] public float ZoomSpeed = 0.05f;
    public float MaxSize = 10f; // 최대 줌아웃 (전체 맵)
    public float MinSize = 3f; // 최대 줌인 (건물 2개)

    [Header("Bounds (유효 타일 영역)")] public Bounds CameraBounds;

    // 외부에서 팝업 등 비활성화 제어용
    [HideInInspector] public bool IsInputLocked = false;

    private Camera _cam;
    private Vector3 _velocity;
    private bool _isPanning;
    private Vector2 _lastPanPos;

    // Pinch
    private float _prevPinchDist;
    private bool _isPinching;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
    }

    void Update()
    {
        if (IsInputLocked) return;

// 유니티 엔진에서 마우스 동작 관련 코드
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
    // 단일 터치 Pan
    // ───────────────────────────────
    void HandlePan()
    {
        Touch touch = Input.GetTouch(0);

        // UI 영역 터치 무시 (기획서 5-2)
        if (IsTouchOverUI(touch.fingerId)) return;

        if (touch.phase == TouchPhase.Began)
        {
            _isPanning = true;
            _lastPanPos = touch.position;
            _velocity = Vector3.zero;
        }
        else if (touch.phase == TouchPhase.Moved && _isPanning)
        {
            // 화면 픽셀 → 월드 좌표 델타 변환
            Vector3 prevWorld = _cam.ScreenToWorldPoint(new Vector3(_lastPanPos.x, _lastPanPos.y, 0));
            Vector3 currWorld = _cam.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));

            Vector3 delta = (prevWorld - currWorld) * PanSpeed;
            delta.z = 0;

            _velocity = delta / Time.deltaTime; // 관성용 속도 기록
            transform.position += delta;

            _lastPanPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            _isPanning = false;
            _isPinching = false;
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
        _velocity *= Reducing; // 매 프레임 Reducing 비율로 감속
    }

    // ───────────────────────────────
    // 두 손가락 Pinch Zoom
    // ───────────────────────────────
    void HandlePinchZoom()
    {
        // Pinch 시작 시 Pan 관성 제거 (기획서 5-4)
        _velocity = Vector3.zero;
        _isPanning = false;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        // UI 위에 있으면 무시
        if (IsTouchOverUI(t0.fingerId) || IsTouchOverUI(t1.fingerId)) return;

        float currentDist = Vector2.Distance(t0.position, t1.position);

        if (!_isPinching)
        {
            _prevPinchDist = currentDist;
            _isPinching = true;
            return;
        }

        float delta = _prevPinchDist - currentDist; // 음수 = 벌림(줌인), 양수 = 모음(줌아웃)
        _cam.orthographicSize = Mathf.Clamp(
            _cam.orthographicSize + delta * ZoomSpeed,
            MinSize,
            MaxSize
        );

        _prevPinchDist = currentDist;
    }

    // ───────────────────────────────
    // 카메라 중심 경계 제한 (기획서 3-4)
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
    // 팝업 시스템 연동용 (외부 호출)
    // ───────────────────────────────
    public void SetInputLock(bool locked)
    {
        IsInputLocked = locked;
        if (locked) _velocity = Vector3.zero;
    }

    // Bounds를 씬에서 확인하기 위한 기즈모
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
        // 왼쪽 클릭 시작
        if (Input.GetMouseButtonDown(0))
        {
            _lastPanPos = Input.mousePosition;
            _velocity = Vector3.zero;
            _isPanning = true;
        }
        // 왼쪽 클릭 유지 중 드래그
        else if (Input.GetMouseButton(0) && _isPanning)
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
        // 클릭 종료
        else if (Input.GetMouseButtonUp(0))
        {
            _isPanning = false;
        }

        // 스크롤 휠 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            _cam.orthographicSize = Mathf.Clamp(
                _cam.orthographicSize - scroll * 5f,
                MinSize, MaxSize
            );
        }

        // 클릭 안 할 때 관성
        if (!Input.GetMouseButton(0))
            ApplyInertia();
    }
}