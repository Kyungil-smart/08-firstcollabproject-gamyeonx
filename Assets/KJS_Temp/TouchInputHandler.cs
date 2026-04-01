using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 터치 입력을 수신하고, UI 판별/오브젝트 선택/카메라 조작 입력을 분배한다.
/// </summary>
public class TouchInputHandler : MonoBehaviour
{
    [SerializeField] private float _dragThreshold = 10f;
    [SerializeField] private float _pinchThreshold = 20f;
    
    [Header("이벤트")]
    public UnityEvent<TouchInteractor> _onObjectSelected;
    public UnityEvent _onSelectionCleared;
    public UnityEvent<Vector2> _onDragDelta;
    public UnityEvent<float> _onPinchDelta;
    public UnityEvent _onDoubleTouchTap;
    
    [Header("디버깅용 마커 출력여부")]
    
    [SerializeField] private bool _isTrackingTouchPos;
    [SerializeField] private GameObject _debugFirstTouchPrefab;
    [SerializeField] private GameObject _debugSecondTouchPrefab;

    private static TouchInputHandler _instance;
    public static TouchInputHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TouchInputHandler>();
            }
            return _instance;
        }
    }

    private MobileInputAction _inputActions;
    private Camera _mainCamera;

    // 터치 상태
    private bool _isPrimaryOnUI;
    private bool _isPrimaryTouching;
    private bool _isDragConfirmed;
    private Vector2 _primaryTouchStartPos;
    private Vector2 _lastDragDelta;
    private bool _isSecondaryTouching;
    private float _previousPinchDistance;
    private float _initialPinchDistance;
    private bool _isPinchConfirmed;

    // 디버그 마커
    private GameObject _debugFirstMarker;
    private GameObject _debugSecondMarker;

    public event Action<TouchInteractor> OnObjectSelected;
    public event Action OnSelectionCleared;
    public event Action OnDragStarted;
    public event Action<Vector2> OnDragDelta;
    public event Action<Vector2> OnDragEnded;
    public event Action<float> OnPinchDelta;

    /// <summary>두 손가락 탭이 발생했을 때 발생한다.</summary>
    public event Action OnDoubleTouchTap;

    // -----------------------------------------------------
    private void Awake()
    {
        if (!TrySetSingleton()) return;
        Init();
    }
    private void OnEnable() => BindInputActions();
    private void OnDisable() => UnbindInputActions();
    private void OnDestroy() => _inputActions?.Dispose();
    // -----------------------------------------------------

    private bool TrySetSingleton()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return false;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        return true;
    }

    private void Init()
    {
        _inputActions = new MobileInputAction();
        _mainCamera = Camera.main;
    }

    private void BindInputActions()
    {
        _inputActions.Touch.Enable();

        _inputActions.Touch.PrimaryTouchContact.started += OnPrimaryTouchStart;
        _inputActions.Touch.PrimaryTouchContact.canceled += OnPrimaryTouchEnd;
        _inputActions.Touch.PrimaryTouchDelta.performed += OnPrimaryTouchDrag;

        _inputActions.Touch.SecondaryTouchContact.started += OnSecondaryTouchStart;
        _inputActions.Touch.SecondaryTouchContact.canceled += OnSecondaryTouchEnd;
        _inputActions.Touch.SecondaryTouchPosition.performed += OnSecondaryTouchMove;

        BindDebugActions();
    }

    private void UnbindInputActions()
    {
        _inputActions.Touch.PrimaryTouchContact.started -= OnPrimaryTouchStart;
        _inputActions.Touch.PrimaryTouchContact.canceled -= OnPrimaryTouchEnd;
        _inputActions.Touch.PrimaryTouchDelta.performed -= OnPrimaryTouchDrag;

        _inputActions.Touch.SecondaryTouchContact.started -= OnSecondaryTouchStart;
        _inputActions.Touch.SecondaryTouchContact.canceled -= OnSecondaryTouchEnd;
        _inputActions.Touch.SecondaryTouchPosition.performed -= OnSecondaryTouchMove;

        UnbindDebugActions();
        _inputActions.Touch.Disable();
    }

    // 터치 입력 처리----------------------
    private void OnPrimaryTouchStart(InputAction.CallbackContext context)
    {
        _isPrimaryOnUI = IsPointerOverUI();
        if (_isPrimaryOnUI) return;

        _isPrimaryTouching = true;
        _isDragConfirmed = false;
        _primaryTouchStartPos = _inputActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
    }

    private void OnPrimaryTouchEnd(InputAction.CallbackContext context)
    {
        // 드래그 중이었으면 관성용 마지막 delta 전달
        if (_isDragConfirmed)
        {
            OnDragEnded?.Invoke(_lastDragDelta);
        }

        // 드래그로 전환되지 않은 짧은 탭 → 오브젝트 선택
        if (_isPrimaryTouching && !_isDragConfirmed && !_isPrimaryOnUI)
        {
            TrySelectObject(_primaryTouchStartPos);
        }

        _isPrimaryTouching = false;
        _isDragConfirmed = false;
        _isPrimaryOnUI = false;
        _lastDragDelta = Vector2.zero;
    }

    private void OnPrimaryTouchDrag(InputAction.CallbackContext context)
    {
        if (_isPrimaryOnUI || !_isPrimaryTouching) return;
        if (_isSecondaryTouching) return;

        if (!_isDragConfirmed)
        {
            Vector2 currentPos = _inputActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
            float distance = Vector2.Distance(_primaryTouchStartPos, currentPos);
            if (distance < _dragThreshold) return;

            _isDragConfirmed = true;
            OnDragStarted?.Invoke();
        }

        Vector2 delta = context.ReadValue<Vector2>();
        _lastDragDelta = delta;
        OnDragDelta?.Invoke(delta);
        _onDragDelta?.Invoke(delta);
    }

    private void OnSecondaryTouchStart(InputAction.CallbackContext context)
    {
        _isSecondaryTouching = true;
        _isPinchConfirmed = false;

        Vector2 primaryPos = _inputActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
        Vector2 secondaryPos = _inputActions.Touch.SecondaryTouchPosition.ReadValue<Vector2>();
        float distance = Vector2.Distance(primaryPos, secondaryPos);
        _initialPinchDistance = distance;
        _previousPinchDistance = distance;
    }

    private void OnSecondaryTouchEnd(InputAction.CallbackContext context)
    {
        // 핀치로 전환되지 않았으면 두 손가락 탭
        if (_isSecondaryTouching && !_isPinchConfirmed)
        {
            OnDoubleTouchTap?.Invoke();
            _onDoubleTouchTap?.Invoke();
        }

        _isSecondaryTouching = false;
        _isPinchConfirmed = false;
    }

    private void OnSecondaryTouchMove(InputAction.CallbackContext context)
    {
        if (!_isSecondaryTouching) return;

        Vector2 primaryPos = _inputActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
        Vector2 secondaryPos = context.ReadValue<Vector2>();
        float currentDistance = Vector2.Distance(primaryPos, secondaryPos);

        // 핀치 확정 전이면 threshold 체크
        if (!_isPinchConfirmed)
        {
            float distanceChange = Mathf.Abs(currentDistance - _initialPinchDistance);
            if (distanceChange < _pinchThreshold)
            {
                _previousPinchDistance = currentDistance;
                return;
            }

            _isPinchConfirmed = true;
        }

        float pinchDelta = currentDistance - _previousPinchDistance;
        _previousPinchDistance = currentDistance;

        OnPinchDelta?.Invoke(pinchDelta);
        _onPinchDelta?.Invoke(pinchDelta);
    }

    // 오브젝트 선택---------------------------------
    private void TrySelectObject(Vector2 screenPos)
    {
        Vector2 worldPos = ScreenToWorld(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            TouchInteractor obj = hit.collider.GetComponent<TouchInteractor>();

            if (obj == null) return;
            OnObjectSelected?.Invoke(obj);
            _onObjectSelected?.Invoke(obj);
            return;
        }

        OnSelectionCleared?.Invoke();
        _onSelectionCleared?.Invoke();
    }

    // 유틸리티---------------------------------------

    /// <summary>
    /// 현재 터치 위치가 UI가 아닌 게임 영역인지 반환한다.
    /// </summary>
    public bool IsInteractableArea()
    {
        if (EventSystem.current == null) return true;
        return !EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z);
        return _mainCamera.ScreenToWorldPoint(pos);
    }

    // 디버그 마커-----------------------------------------
    private void BindDebugActions()
    {
        if (!_isTrackingTouchPos) return;

        _inputActions.Touch.PrimaryTouchContact.started += OnDebugFirstTouchStart;
        _inputActions.Touch.PrimaryTouchPosition.performed += OnDebugFirstTouchTracking;
        _inputActions.Touch.PrimaryTouchContact.canceled += OnDebugFirstTouchEnd;

        _inputActions.Touch.SecondaryTouchContact.started += OnDebugSecondTouchStart;
        _inputActions.Touch.SecondaryTouchPosition.performed += OnDebugSecondTouchTracking;
        _inputActions.Touch.SecondaryTouchContact.canceled += OnDebugSecondTouchEnd;
    }

    private void UnbindDebugActions()
    {
        if (!_isTrackingTouchPos) return;

        _inputActions.Touch.PrimaryTouchContact.started -= OnDebugFirstTouchStart;
        _inputActions.Touch.PrimaryTouchPosition.performed -= OnDebugFirstTouchTracking;
        _inputActions.Touch.PrimaryTouchContact.canceled -= OnDebugFirstTouchEnd;

        _inputActions.Touch.SecondaryTouchContact.started -= OnDebugSecondTouchStart;
        _inputActions.Touch.SecondaryTouchPosition.performed -= OnDebugSecondTouchTracking;
        _inputActions.Touch.SecondaryTouchContact.canceled -= OnDebugSecondTouchEnd;
    }

    private void OnDebugFirstTouchStart(InputAction.CallbackContext context)
    {
        Vector2 screenPos = _inputActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
        Vector3 worldPos = ScreenToWorld(screenPos);
        _debugFirstMarker = Instantiate(_debugFirstTouchPrefab, worldPos, Quaternion.identity);
    }

    private void OnDebugFirstTouchTracking(InputAction.CallbackContext context)
    {
        if (_debugFirstMarker == null) return;

        Vector2 screenPos = context.ReadValue<Vector2>();
        _debugFirstMarker.transform.position = ScreenToWorld(screenPos);
    }

    private void OnDebugFirstTouchEnd(InputAction.CallbackContext context)
    {
        if (_debugFirstMarker == null) return;

        Destroy(_debugFirstMarker);
        _debugFirstMarker = null;
    }

    private void OnDebugSecondTouchStart(InputAction.CallbackContext context)
    {
        Vector2 screenPos = _inputActions.Touch.SecondaryTouchPosition.ReadValue<Vector2>();
        Vector3 worldPos = ScreenToWorld(screenPos);
        _debugSecondMarker = Instantiate(_debugSecondTouchPrefab, worldPos, Quaternion.identity);
    }

    private void OnDebugSecondTouchTracking(InputAction.CallbackContext context)
    {
        if (_debugSecondMarker == null) return;

        Vector2 screenPos = context.ReadValue<Vector2>();
        _debugSecondMarker.transform.position = ScreenToWorld(screenPos);
    }

    private void OnDebugSecondTouchEnd(InputAction.CallbackContext context)
    {
        if (_debugSecondMarker == null) return;

        Destroy(_debugSecondMarker);
        _debugSecondMarker = null;
    }
}
