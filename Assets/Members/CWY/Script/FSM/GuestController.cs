using UnityEngine;

/// <summary>
/// 손님 1명의 전체 흐름을 관리하는 메인 컨트롤러
/// FSM, 런타임 상태값, 시설 선택, 외부 시스템 연결 지점을 담당
/// </summary>
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
    [Tooltip("배회 중 컨디션 증가 주기. 기획 기준 2초")]
    [SerializeField] private float _wanderNeedTickInterval = 2f;

    [Tooltip("배회 중 이벤트 판정 주기")]
    [SerializeField] private float _wanderEventCheckInterval = 1f;

    [Tooltip("배회 중 시설 이용 이벤트 발생 확률(%)")]
    [SerializeField, Range(0f, 100f)] private float _facilityUseEventChancePercent = 20f;

    [Header("이용 설정")]
    [Tooltip("시설 이용 중 틱 적용 주기")]
    [SerializeField] private float _useEffectTickInterval = 1f;

    [Header("퇴장 설정")]
    [Tooltip("시설 1회 이용 완료 시 증가하는 퇴장 확률(%)")]
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

    private void Update()
    {
        _stateMachine?.Update();
    }

    private void Initialize()
    {
        _utilityEvaluator = new GuestUtilityEvaluator();
        _stateMachine = new GuestStateMachine();

        _wanderState = new GuestWanderState(this);
        _decideState = new GuestDecideState(this);
        _moveState = new GuestMoveState(this);
        _waitState = new GuestWaitState(this);
        _useState = new GuestUseState(this);
        _exitState = new GuestExitState(this);

        LoadGuestData();
        _stateMachine.ChangeState(_wanderState);

        Log("[GuestController] 초기화 완료. 시작 상태 = Wander");
    }

    private void LoadGuestData()
    {
        if (_guestDataDatabase == null)
        {
            Debug.LogError("[GuestController] GuestDataDatabaseSO가 비어 있습니다.");
            return;
        }

        GuestDataRow row = _guestDataDatabase.GetGuestDataByVisitorID(_visitorID);

        if (row == null)
        {
            Debug.LogError($"[GuestController] VisitorID={_visitorID} 데이터를 찾지 못했습니다.");
            return;
        }

        _guestStates.Initialize(
            row.VisitorID,
            row.Hunger,
            row.Thirst,
            row.Fatigue,
            row.Satisfaction
        );

        Log($"[GuestController] 손님 데이터 로드 완료 | {row.GetDebugText()}");
    }

    public void EvaluateCurrentNeed()
    {
        if (_utilityEvaluator == null)
        {
            Debug.LogError("[GuestController] UtilityEvaluator가 비어 있습니다.");
            return;
        }

        CurrentNeedType = _utilityEvaluator.EvaluateHighestNeed(_guestStates);
        CurrentTargetFacilityType = _utilityEvaluator.EvaluateTargetFacilityType(_guestStates);

        Log($"[GuestController] Need 평가 완료 | Need={CurrentNeedType}, TargetFacilityType={CurrentTargetFacilityType}");
    }

    /// <summary>
    /// 현재 Need 기준으로 목표 시설을 찾는다.
    /// 현재는 첫 번째 selectable 시설을 반환.
    /// 나중에 시설 위치/거리 시스템과 연결되면 '가장 가까운 시설'로 교체.
    /// </summary>
    public bool TryFindTargetFacility()
    {
        if (_facilityEffectDatabase == null)
        {
            Debug.LogError("[GuestController] FacilityEffectDatabaseSO가 비어 있습니다.");
            return false;
        }

        if (CurrentTargetFacilityType == EFacilityType.None)
        {
            Debug.LogWarning("[GuestController] 목표 시설 타입이 없습니다.");
            return false;
        }

        FacilityEffectRow targetRow = _facilityEffectDatabase.GetFirstSelectableEffectByType(CurrentTargetFacilityType);

        if (targetRow == null)
        {
            Debug.LogWarning($"[GuestController] 선택 가능한 목표 시설이 없습니다. TargetFacilityType={CurrentTargetFacilityType}");
            return false;
        }

        SetCurrentTargetFacility(targetRow.FacilityID, targetRow.FacilityType);
        return true;
    }

    public void SetCurrentTargetFacility(int facilityID, EFacilityType facilityType)
    {
        CurrentTargetFacilityID = facilityID;
        CurrentTargetFacilityType = facilityType;

        ResetMovementAndFacilityFlags();

        Log($"[GuestController] 목표 시설 설정 | FacilityID={facilityID}, FacilityType={facilityType}");
    }

    public void ClearCurrentFacilityContext()
    {
        CurrentTargetFacilityID = -1;
        CurrentTargetFacilityType = EFacilityType.None;
        CurrentNeedType = EGuestNeedType.None;

        ResetMovementAndFacilityFlags();

        Log("[GuestController] 현재 시설 문맥 초기화");
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
        Log($"[GuestController] HasArrivedAtFacility={value}");
    }

    public void SetCanUseFacility(bool value)
    {
        CanUseFacility = value;
        Log($"[GuestController] CanUseFacility={value}");
    }

    public void SetShouldWaitForFacility(bool value)
    {
        ShouldWaitForFacility = value;
        Log($"[GuestController] ShouldWaitForFacility={value}");
    }

    public void SetMovementFailed(bool value)
    {
        HasMovementFailed = value;
        Log($"[GuestController] HasMovementFailed={value}");
    }

    public void SetFacilityUseFailed(bool value)
    {
        HasFacilityUseFailed = value;
        Log($"[GuestController] HasFacilityUseFailed={value}");
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
            Log($"[GuestController] 일반 퇴장 이벤트 발생 | ExitChance={CurrentExitChancePercent}%");
        }

        return triggered;
    }

    public void ApplyCurrentFacilityEffect()
    {
        if (_facilityEffectDatabase == null)
        {
            Debug.LogError("[GuestController] FacilityEffectDatabaseSO가 비어 있습니다.");
            return;
        }

        if (CurrentTargetFacilityID < 0)
        {
            Debug.LogWarning("[GuestController] CurrentTargetFacilityID가 유효하지 않습니다.");
            return;
        }

        FacilityEffectRow row = _facilityEffectDatabase.GetEffectByFacilityID(CurrentTargetFacilityID);

        if (row == null)
        {
            Debug.LogWarning($"[GuestController] FacilityID={CurrentTargetFacilityID} 효과 데이터를 찾지 못했습니다.");
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

        Log($"[GuestController] 시설 이용 완료 | UseCount={FacilityUseCount}, ExitChance={CurrentExitChancePercent}%");

        // TODO: 재화/평판 시스템 연결 지점
        // 시설 이용 완료 시점에 골드, 평판 반영

        ClearCurrentFacilityContext();
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

    public void NotifyTurnEnded()
    {
        IsTurnEnding = true;
        Log("[GuestController] 턴 종료 통보 수신");

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
        Log("[GuestController] 퇴장 완료. 오브젝트 제거");
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

    [ContextMenu("디버그/도착 처리")]
    private void DebugArrive()
    {
        SetArrivedAtFacility(true);
    }

    [ContextMenu("디버그/즉시 이용 가능")]
    private void DebugCanUse()
    {
        SetCanUseFacility(true);
    }

    [ContextMenu("디버그/대기 필요")]
    private void DebugShouldWait()
    {
        SetShouldWaitForFacility(true);
    }

    [ContextMenu("디버그/턴 종료")]
    private void DebugTurnEnd()
    {
        NotifyTurnEnded();
    }

    private void Log(string message)
    {
        if (_enableDebugLog)
        {
            Debug.Log(message);
        }
    }
}