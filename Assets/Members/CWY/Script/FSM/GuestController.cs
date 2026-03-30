using UnityEngine;

/// <summary>
/// 손님 1명의 전체 흐름을 관리하는 메인 컨트롤러
/// FSM, 런타임 상태값, 시설 선택, 외부 시스템 연결 지점을 담당
/// </summary>
[RequireComponent(typeof(GuestMovementAgent))]
[RequireComponent(typeof(GuestRoadWanderSelector))]
[RequireComponent(typeof(GuestEntryFlowHandler))]
[RequireComponent(typeof(GuestExitFlowHandler))]
public class GuestController : MonoBehaviour
{
    [Header("손님 식별값")]
    [SerializeField] private int _visitorID = 1;

    [Header("DB 참조")]
    [SerializeField] private GuestDataDatabaseSO _guestDataDatabase;
    [SerializeField] private FacilityEffectDatabaseSO _facilityEffectDatabase;

    [Header("런타임 상태")]
    [SerializeField] private GuestStates _guestStates = new GuestStates();

    [Header("배회 설정")]
    [SerializeField] private float _wanderNeedTickInterval = 2f;
    [SerializeField] private float _wanderEventCheckInterval = 1f;
    [SerializeField, Range(0f, 100f)] private float _facilityUseEventChancePercent = 20f;

    [Header("이용 설정")]
    [SerializeField] private float _useEffectTickInterval = 1f;

    [Header("퇴장 설정")]
    [SerializeField, Range(0f, 100f)] private float _exitChanceIncreasePerUse = 3f;

    [Header("디버그")]
    [SerializeField] private bool _enableDebugLog = true;

    public GuestStates GuestStates => _guestStates;
    public float WanderNeedTickInterval => _wanderNeedTickInterval;
    public float WanderEventCheckInterval => _wanderEventCheckInterval;
    public float UseEffectTickInterval => _useEffectTickInterval;

    public EGuestNeedType CurrentNeedType { get; private set; } = EGuestNeedType.None;
    public EFacilityType CurrentTargetFacilityType { get; private set; } = EFacilityType.None;
    public int CurrentTargetFacilityID { get; private set; } = -1;

    public bool HasArrivedAtFacility { get; private set; }
    public bool CanUseFacility { get; private set; }
    public bool ShouldWaitForFacility { get; private set; }
    public bool HasMovementFailed { get; private set; }
    public bool HasFacilityUseFailed { get; private set; }

    public bool IsTurnEnding { get; private set; }
    public int FacilityUseCount { get; private set; }
    public float CurrentExitChancePercent { get; private set; }

    public bool IsInsideFacility { get; private set; }
    public FacilityRuntime CurrentFacilityRuntime { get; private set; }

    public GuestMovementAgent MovementAgent { get; private set; }
    public GuestRoadWanderSelector WanderSelector { get; private set; }

    // 추가: 입장/퇴장 흐름 핸들러
    public GuestEntryFlowHandler EntryFlowHandler { get; private set; }
    public GuestExitFlowHandler ExitFlowHandler { get; private set; }

    // 추가: 입장 완료 전 / 퇴장 연출 중 제어용
    public bool HasEnteredGuild { get; private set; }
    public bool IsExitFlowRunning { get; private set; }

    private GuestUtilityEvaluator _utilityEvaluator;
    private GuestStateMachine _stateMachine;

    private GuestWanderState _wanderState;
    private GuestDecideState _decideState;
    private GuestMoveState _moveState;
    private GuestWaitState _waitState;
    private GuestUseState _useState;
    private GuestExitState _exitState;

    [Header("골드 정상")]
    [SerializeField] private GoldTest _goldTest;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (MovementAgent != null)
        {
            MovementAgent.OnMoveCompleted += HandleMoveCompleted;
            MovementAgent.OnMoveFailed += HandleMoveFailed;
        }
    }

    private void OnDisable()
    {
        if (MovementAgent != null)
        {
            MovementAgent.OnMoveCompleted -= HandleMoveCompleted;
            MovementAgent.OnMoveFailed -= HandleMoveFailed;
        }
    }

    private void Update()
    {
        // 입장 전에는 일반 FSM 정지
        if (!HasEnteredGuild)
        {
            return;
        }

        // 퇴장 연출 중에는 일반 FSM 정지
        if (IsExitFlowRunning)
        {
            return;
        }

        _stateMachine?.Update();
    }

    private void Initialize()
    {
        MovementAgent = GetComponent<GuestMovementAgent>();
        WanderSelector = GetComponent<GuestRoadWanderSelector>();

        // 추가
        EntryFlowHandler = GetComponent<GuestEntryFlowHandler>();
        ExitFlowHandler = GetComponent<GuestExitFlowHandler>();

        _utilityEvaluator = new GuestUtilityEvaluator();
        _stateMachine = new GuestStateMachine();

        _wanderState = new GuestWanderState(this);
        _decideState = new GuestDecideState(this);
        _moveState = new GuestMoveState(this);
        _waitState = new GuestWaitState(this);
        _useState = new GuestUseState(this);
        _exitState = new GuestExitState(this);

        LoadGuestData();

        // 변경: 바로 Wander 시작하지 않음
        HasEnteredGuild = false;
        IsExitFlowRunning = false;

        Log("[GuestController] 초기화 완료");
    }

    private void LoadGuestData()
    {
        if (_guestDataDatabase == null)
        {
            return;
        }

        GuestDataRow row = _guestDataDatabase.GetGuestDataByVisitorID(_visitorID);

        if (row == null)
        {
            return;
        }

        // 만족도 제거 반영: 3개 컨디션만 초기화
        _guestStates.Initialize(row.VisitorID, row.Hunger, row.Thirst, row.Fatigue);
    }

    // 추가: 스포너가 생성 직후 호출
    public void SetupSpawn(int visitorID)
    {
        _visitorID = visitorID;
        LoadGuestData();

        HasEnteredGuild = false;
        IsExitFlowRunning = false;

        if (EntryFlowHandler == null)
        {
            Debug.LogWarning("[GuestController] EntryFlowHandler가 없습니다.");
            return;
        }

        EntryFlowHandler.BeginEntryFlow();
        Log($"[GuestController] 스폰 세팅 완료 | VisitorID={_visitorID}");
    }

    // 추가: 입장 연출 완료 시 호출
    public void HandleEntryFlowCompleted()
    {
        HasEnteredGuild = true;
        ChangeToWanderState();

        Log("[GuestController] 길드 입장 완료 -> Wander 상태 시작");
    }

    // 추가: 입장 연출 실패 시 호출
    public void HandleEntryFlowFailed()
    {
        Debug.LogWarning("[GuestController] 입장 연출 실패");
        Destroy(gameObject);
    }

    // 추가: 퇴장 연출 완료 시 호출
    public void HandleExitFlowCompleted()
    {
        IsExitFlowRunning = false;
        CompleteExit();
    }

    // 추가: 퇴장 연출 실패 시 호출
    public void HandleExitFlowFailed()
    {
        IsExitFlowRunning = false;
        CompleteExit();
    }

    public void EvaluateCurrentNeed()
    {
        CurrentNeedType = _utilityEvaluator.EvaluateHighestNeed(_guestStates);
        CurrentTargetFacilityType = _utilityEvaluator.EvaluateTargetFacilityType(_guestStates);
    }

    public bool TryFindTargetFacility()
    {
        if (_facilityEffectDatabase == null)
        {
            return false;
        }

        if (CurrentTargetFacilityType == EFacilityType.None)
        {
            return false;
        }

        FacilityEffectRow targetRow = _facilityEffectDatabase.GetFirstSelectableEffectByType(CurrentTargetFacilityType);

        if (targetRow == null)
        {
            return false;
        }

        SetCurrentTargetFacility(targetRow.FacilityID, targetRow.FacilityType);
        return true;
    }

    public void SetCurrentTargetFacility(int facilityID, EFacilityType facilityType)
    {
        CurrentTargetFacilityID = facilityID;
        CurrentTargetFacilityType = facilityType;
        CurrentFacilityRuntime = FacilityRegistry.Instance != null
            ? FacilityRegistry.Instance.GetFacility(facilityID)
            : null;

        ResetMovementAndFacilityFlags();
    }

    public void ClearCurrentFacilityContext()
    {
        CurrentTargetFacilityID = -1;
        CurrentTargetFacilityType = EFacilityType.None;
        CurrentNeedType = EGuestNeedType.None;
        CurrentFacilityRuntime = null;

        ResetMovementAndFacilityFlags();
    }

    public void ResetMovementAndFacilityFlags()
    {
        HasArrivedAtFacility = false;
        CanUseFacility = false;
        ShouldWaitForFacility = false;
        HasMovementFailed = false;
        HasFacilityUseFailed = false;
    }

    public void SetArrivedAtFacility(bool value)
    {
        HasArrivedAtFacility = value;
    }

    public void SetCanUseFacility(bool value)
    {
        CanUseFacility = value;
    }

    public void SetShouldWaitForFacility(bool value)
    {
        ShouldWaitForFacility = value;
    }

    public void SetMovementFailed(bool value)
    {
        HasMovementFailed = value;
    }

    public void SetFacilityUseFailed(bool value)
    {
        HasFacilityUseFailed = value;
    }

    public void ApplyWanderNeedTick()
    {
        _guestStates.IncreaseAllNeedsByWanderTick();
    }

    public bool ShouldStartFacilitySearchNow()
    {
        if (_guestStates.HasAnyNeedReachedMax())
        {
            Log("[GuestController] Need가 100에 도달해서 즉시 시설 탐색");
            return true;
        }

        bool triggered = Random.Range(0f, 100f) < _facilityUseEventChancePercent;

        if (triggered)
        {
            Log("[GuestController] 시설 이용 이벤트 발생");
        }

        return triggered;
    }

    public bool ShouldExitFromWander()
    {
        if (FacilityUseCount <= 0)
        {
            return false;
        }

        bool triggered = Random.Range(0f, 100f) < CurrentExitChancePercent;

        if (triggered)
        {
            Debug.Log($"[GuestController] 일반 퇴장 이벤트 발생 | ExitChance={CurrentExitChancePercent}%");
        }

        return triggered;
    }

    public void ApplyCurrentFacilityEffect()
    {
        if (_facilityEffectDatabase == null)
        {
            return;
        }

        FacilityEffectRow row = _facilityEffectDatabase.GetEffectByFacilityID(CurrentTargetFacilityID);

        if (row == null)
        {
            return;
        }

        _guestStates.ApplyFacilityEffect(row);
    }

    public bool IsCurrentFacilityGoalReached()
    {
        if (CurrentTargetFacilityType == EFacilityType.None)
        {
            return false;
        }

        EGuestNeedType targetNeed = GetNeedTypeByFacilityType(CurrentTargetFacilityType);

        if (targetNeed == EGuestNeedType.None)
        {
            return false;
        }

        return _guestStates.GetNeedValue(targetNeed) <= 0;
    }

    public void FinishCurrentFacilityUse()
    {
        EGuestNeedType targetNeed = GetNeedTypeByFacilityType(CurrentTargetFacilityType);

        if (targetNeed != EGuestNeedType.None)
        {
            _guestStates.SetNeedValue(targetNeed, 0);
        }

        FacilityUseCount++;
        CurrentExitChancePercent = FacilityUseCount * _exitChanceIncreasePerUse;
    }

    public EGuestNeedType GetNeedTypeByFacilityType(EFacilityType facilityType)
    {
        switch (facilityType)
        {
            case EFacilityType.Restaurant:
                return EGuestNeedType.Hunger;
            case EFacilityType.VendingMachine:
                return EGuestNeedType.Thirst;
            case EFacilityType.HotSpring:
                return EGuestNeedType.Fatigue;
            default:
                return EGuestNeedType.None;
        }
    }

    public bool RequestMoveToFacilityEntrance()
    {
        if (CurrentFacilityRuntime == null)
        {
            SetMovementFailed(true);
            return false;
        }

        bool requested = MovementAgent.MoveToRoadCell(CurrentFacilityRuntime.EntranceRoadCell);

        if (!requested)
        {
            SetMovementFailed(true);
        }

        return requested;
    }

    public bool RequestRandomWanderMove()
    {
        if (WanderSelector == null)
        {
            return false;
        }

        bool found = WanderSelector.TryGetRandomRoadCell(out Vector3Int targetRoadCell);

        if (!found)
        {
            return false;
        }

        bool requested = MovementAgent.MoveToRoadCell(targetRoadCell);

        if (!requested)
        {
            Debug.Log("[GuestController] 배회 이동 요청 실패");
            return false;
        }

        return true;
    }

    public void EnterFacility(FacilityRuntime facility)
    {
        if (facility == null)
        {
            SetFacilityUseFailed(true);
            return;
        }

        if (facility.FacilityID != CurrentTargetFacilityID)
        {
            return;
        }

        IsInsideFacility = true;
        CurrentFacilityRuntime = facility;

        MovementAgent.StopMove();
        MovementAgent.TeleportTo(facility.InteriorEntryPoint);
        SetArrivedAtFacility(true);

        if (facility.CanUseImmediately)
        {
            SetCanUseFacility(true);
        }
        else if (facility.SupportsQueue)
        {
            SetShouldWaitForFacility(true);
        }
        else
        {
            SetFacilityUseFailed(true);
        }

        Debug.Log($"[GuestController] 시설 내부 진입 완료 | FacilityID={facility.FacilityID}");
    }

    public void MoveToFacilityUsePoint()
    {
        if (CurrentFacilityRuntime == null || CurrentFacilityRuntime.UsePoint == null)
        {
            SetFacilityUseFailed(true);
            return;
        }

        MovementAgent.MoveInsideTo(CurrentFacilityRuntime.UsePoint);
    }

    public void ExitFacilityToOutside()
    {
        if (CurrentFacilityRuntime != null && CurrentFacilityRuntime.OutsideExitPoint != null)
        {
            MovementAgent.TeleportTo(CurrentFacilityRuntime.OutsideExitPoint);

            // 연동준이 추가
            int gold = CurrentFacilityRuntime.GetPrice();
            _goldTest.PayMoney(gold);
        }

        IsInsideFacility = false;
        ClearCurrentFacilityContext();
    }

    public void NotifyTurnEnded()
    {
        IsTurnEnding = true;

        if (!IsCurrentStateUse())
        {
            ChangeToExitState();
        }
    }

    public bool IsCurrentStateUse()
    {
        return _stateMachine != null && _stateMachine.CurrentState == _useState;
    }

    public void CompleteExit()
    {
        if (_goldTest != null)
        {
            // _goldTest.PayMoney(10);
        }
        Destroy(gameObject);
    }

    public void ChangeToWanderState()
    {
        _stateMachine.ChangeState(_wanderState);
    }

    public void ChangeToDecideState()
    {
        _stateMachine.ChangeState(_decideState);
    }

    public void ChangeToMoveState()
    {
        _stateMachine.ChangeState(_moveState);
    }

    public void ChangeToWaitState()
    {
        _stateMachine.ChangeState(_waitState);
    }

    public void ChangeToUseState()
    {
        _stateMachine.ChangeState(_useState);
    }

    public void ChangeToExitState()
    {
        // 기존 기능 확장: 바로 ExitState로만 넘기지 않고 퇴장 연출 시작
        if (IsExitFlowRunning)
        {
            return;
        }

        IsExitFlowRunning = true;

        if (ExitFlowHandler != null)
        {
            ExitFlowHandler.BeginExitFlow();
            return;
        }

        _stateMachine.ChangeState(_exitState);
    }

    private void HandleMoveCompleted()
    {
        Debug.Log("[GuestController] 이동 완료 이벤트 수신");
    }

    private void HandleMoveFailed()
    {
        SetMovementFailed(true);
    }

    private void Log(string message)
    {
        if (_enableDebugLog)
        {
            Debug.Log(message);
        }
    }
}

/*
[Unity 구현 방법]
1. Guest 프리팹에 GuestEntryFlowHandler, GuestExitFlowHandler를 추가합니다.
2. GuestSpawner가 손님 생성 직후 SetupSpawn(visitorID)를 호출합니다.
3. 이제 손님은 생성 직후 바로 Wander에 들어가지 않고,
   EntryFlowHandler 완료 후 Wander 상태로 전환됩니다.
4. 퇴장 시에는 ExitFlowHandler가 연결되어 있으면 퇴장 연출을 먼저 실행합니다.
5. 기존 시설 탐색, 이용, 골드 지급, 상태 변화 기능은 그대로 유지됩니다.
*/