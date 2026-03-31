using UnityEngine;

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

    [Header("골드")]
    [SerializeField] private GoldTest _goldTest;

    public GuestStates GuestStates => _guestStates;
    public float WanderNeedTickInterval => _wanderNeedTickInterval;
    public float WanderEventCheckInterval => _wanderEventCheckInterval;
    public float UseEffectTickInterval => _useEffectTickInterval;

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
    public bool IsLeavingFacility { get; private set; }
    public bool HasFinishedFacilityUse { get; private set; }

    public FacilityRuntime CurrentFacilityRuntime { get; private set; }
    public Transform AssignedUsePoint { get; private set; }

    public GuestMovementAgent MovementAgent { get; private set; }
    public GuestRoadWanderSelector WanderSelector { get; private set; }
    public GuestEntryFlowHandler EntryFlowHandler { get; private set; }
    public GuestExitFlowHandler ExitFlowHandler { get; private set; }

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

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (MovementAgent != null)
        {
            MovementAgent.OnMoveFailed += HandleMoveFailed;
        }
    }

    private void OnDisable()
    {
        if (MovementAgent != null)
        {
            MovementAgent.OnMoveFailed -= HandleMoveFailed;
        }
    }

    private void Update()
    {
        if (!HasEnteredGuild)
        {
            return;
        }

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

        _guestStates.Initialize(row.VisitorID, row.Hunger, row.Thirst, row.Fatigue);
    }

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

    public void HandleEntryFlowCompleted()
    {
        HasEnteredGuild = true;
        ChangeToWanderState();

        Log("[GuestController] 길드 입장 완료 -> Wander 상태 시작");
    }

    public void HandleEntryFlowFailed()
    {
        Debug.LogWarning("[GuestController] 입장 연출 실패");
        Destroy(gameObject);
    }

    public void HandleExitFlowCompleted()
    {
        IsExitFlowRunning = false;
        CompleteExit();
    }

    public void HandleExitFlowFailed()
    {
        IsExitFlowRunning = false;
        CompleteExit();
    }

    public void EvaluateCurrentNeed()
    {
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
        CurrentFacilityRuntime = null;
        AssignedUsePoint = null;

        IsInsideFacility = false;
        IsLeavingFacility = false;
        HasFinishedFacilityUse = false;

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
        HasFinishedFacilityUse = true;
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
        IsLeavingFacility = false;
        HasFinishedFacilityUse = false;
        CurrentFacilityRuntime = facility;

        MovementAgent.StopMove();

        if (facility.InteriorEntryPoint != null)
        {
            MovementAgent.TeleportTo(facility.InteriorEntryPoint);
        }

        SetArrivedAtFacility(true);

        bool requestResult = facility.TryRequestUse(this, out Transform assignedUsePoint, out bool isQueued);

        if (!requestResult)
        {
            SetFacilityUseFailed(true);
            return;
        }

        if (assignedUsePoint != null)
        {
            NotifyUseSlotAssigned(assignedUsePoint);
        }
        else if (isQueued)
        {
            NotifyQueuedForFacility();
        }
        else
        {
            SetFacilityUseFailed(true);
        }

        Debug.Log($"[GuestController] 시설 내부 진입 완료 | FacilityID={facility.FacilityID}");
    }

    public void NotifyUseSlotAssigned(Transform usePoint)
    {
        AssignedUsePoint = usePoint;
        SetCanUseFacility(true);
        SetShouldWaitForFacility(false);
        SetFacilityUseFailed(false);

        Debug.Log($"[GuestController] 좌석 배정 완료 | UsePoint={usePoint.name}");
    }

    public void NotifyQueuedForFacility()
    {
        AssignedUsePoint = null;
        SetCanUseFacility(false);
        SetShouldWaitForFacility(true);
        SetFacilityUseFailed(false);

        if (CurrentFacilityRuntime != null && CurrentFacilityRuntime.WaitPoint != null)
        {
            MovementAgent.MoveInsideTo(CurrentFacilityRuntime.WaitPoint);
        }

        Debug.Log("[GuestController] 대기열 등록 완료");
    }

    public void MoveToAssignedUsePoint()
    {
        if (AssignedUsePoint == null)
        {
            SetFacilityUseFailed(true);
            return;
        }

        MovementAgent.MoveInsideTo(AssignedUsePoint);
    }

    public bool BeginFacilityLeave()
    {
        if (!IsInsideFacility)
        {
            return false;
        }

        if (CurrentFacilityRuntime == null)
        {
            SetFacilityUseFailed(true);
            return false;
        }

        if (CurrentFacilityRuntime.FacilityExitPoint == null)
        {
            SetFacilityUseFailed(true);
            Debug.LogWarning("[GuestController] FacilityExitPoint가 없습니다.");
            return false;
        }

        IsLeavingFacility = true;
        MovementAgent.StopMove();
        MovementAgent.MoveInsideTo(CurrentFacilityRuntime.FacilityExitPoint);

        Debug.Log("[GuestController] 시설 내부 출구 이동 시작");
        return true;
    }

    public void HandleReachedFacilityExitTrigger()
    {
        if (!IsInsideFacility)
        {
            return;
        }

        if (!IsLeavingFacility)
        {
            return;
        }

        if (CurrentFacilityRuntime != null)
        {
            CurrentFacilityRuntime.ReleaseGuest(this);
        }

        if (CurrentFacilityRuntime != null && CurrentFacilityRuntime.OutsideExitPoint != null)
        {
            MovementAgent.StopMove();
            MovementAgent.TeleportTo(CurrentFacilityRuntime.OutsideExitPoint);
        }

        if (HasFinishedFacilityUse && _goldTest != null && CurrentFacilityRuntime != null)
        {
            int gold = CurrentFacilityRuntime.GetPrice();
            _goldTest.PayMoney(gold);
        }

        bool shouldExitToGuild = IsTurnEnding;

        ClearCurrentFacilityContext();

        if (shouldExitToGuild)
        {
            StartGuildExitFlow();
        }
        else
        {
            ChangeToWanderState();
        }
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
        _stateMachine.ChangeState(_exitState);
    }

    public void StartGuildExitFlow()
    {
        if (IsExitFlowRunning)
        {
            return;
        }

        if (CurrentFacilityRuntime != null)
        {
            CurrentFacilityRuntime.ReleaseGuest(this);
        }

        IsExitFlowRunning = true;

        if (ExitFlowHandler != null)
        {
            ExitFlowHandler.BeginExitFlow();
            return;
        }

        CompleteExit();
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