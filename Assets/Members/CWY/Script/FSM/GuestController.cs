using UnityEngine;

/// <summary>
/// Main controller for one guest.
/// This class connects guest data, runtime states, utility AI, and FSM states.
/// </summary>
public class GuestController : MonoBehaviour
{
    [Header("Guest Identity")]
    // 이 손님이 사용할 VisitorID
    [SerializeField] private int _visitorID = 1;

    [Header("Database References")]
    // 손님 설정 데이터 DB
    [SerializeField] private GuestDataDatabaseSO _guestDataDatabase;

    // 시설 효과 데이터 DB
    [SerializeField] private FacilityEffectDatabaseSO _facilityEffectDatabase;

    [Header("Runtime Guest States")]
    // 현재 손님 상태값
    [SerializeField] private GuestStates _guestStates = new GuestStates();

    [Header("State Durations")]
    // Idle 상태 유지 시간
    [SerializeField] private float _idleDuration = 2f;

    // 시설 이용 시간
    [SerializeField] private float _useDuration = 3f;

    // 현재 손님 상태 데이터 getter
    public GuestStates GuestStates => _guestStates;

    // IdleState에서 사용하는 대기 시간
    public float IdleDuration => _idleDuration;

    // WanderState에서 사용하는 배회 시간
    public float WanderDuration { get; private set; } = 3f;

    // UseState에서 사용하는 시설 이용 시간
    public float UseDuration => _useDuration;

    // Utility AI가 판단한 현재 욕구
    public EGuestNeedType CurrentNeedType { get; private set; } = EGuestNeedType.None;

    // Utility AI가 판단한 현재 목표 시설 타입
    public EFacilityType CurrentTargetFacilityType { get; private set; } = EFacilityType.None;

    // 실제 이동 완료 여부
    public bool HasArrivedAtFacility { get; private set; }

    // 시설 사용 가능 여부
    public bool CanUseFacility { get; private set; }

    // 현재 목표 시설 ID
    public int CurrentTargetFacilityID { get; private set; } = -1;

    // Utility AI 평가기
    private GuestUtilityEvaluator _utilityEvaluator;

    // 상태 머신
    private GuestStateMachine _stateMachine;

    // FSM 상태들
    private GuestIdleState _idleState;
    private GuestWanderState _wanderState;
    private GuestDecideState _decideState;
    private GuestMoveState _moveState;
    private GuestWaitState _waitState;
    private GuestUseState _useState;

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        // 현재 상태 업데이트
        _stateMachine?.Update();

        // 손님 상태값을 시간에 따라 변화시킴
        UpdateGuestStatesOverTime();
    }

    /// <summary>
    /// 초기화
    /// </summary>
    private void Initialize()
    {
        // Utility AI 생성
        _utilityEvaluator = new GuestUtilityEvaluator();

        // 상태 머신 생성
        _stateMachine = new GuestStateMachine();

        // FSM 상태 객체 생성
        _idleState = new GuestIdleState(this);
        _wanderState = new GuestWanderState(this);
        _decideState = new GuestDecideState(this);
        _moveState = new GuestMoveState(this);
        _waitState = new GuestWaitState(this);
        _useState = new GuestUseState(this);

        // VisitorID 기준으로 손님 설정 데이터 로드
        LoadGuestData();

        // 초기 상태는 Idle
        _stateMachine.ChangeState(_idleState);

        Debug.Log("[GuestController] Initialized.");
    }

    /// <summary>
    /// GuestDataDatabase에서 VisitorID에 맞는 데이터를 읽어와 초기화
    /// </summary>
    private void LoadGuestData()
    {
        if (_guestDataDatabase == null)
        {
            Debug.LogError("[GuestController] GuestDataDatabase is missing.");
            return;
        }

        GuestDataRow row = _guestDataDatabase.GetGuestDataByVisitorID(_visitorID);

        if (row == null)
        {
            Debug.LogError($"[GuestController] GuestDataRow not found. VisitorID: {_visitorID}");
            return;
        }

        // 현재 상태값 초기화
        _guestStates.Initialize(
            row.VisitorID,
            row.Hunger,
            row.Thirst,
            row.Fatigue,
            row.Cleanliness,
            row.Satisfaction
        );

        // Wander 시간은 시트 데이터에서 가져옴
        WanderDuration = row.WanderDuration;

        Debug.Log($"[GuestController] Guest data loaded. VisitorID: {_visitorID}");
    }

    /// <summary>
    /// 시간에 따라 손님 상태값 변화
    /// </summary>
    private void UpdateGuestStatesOverTime()
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

        // 초당 변화량 적용
        _guestStates.AddHunger(Mathf.RoundToInt(row.HungerDeltaPerSecond * Time.deltaTime));
        _guestStates.AddThirst(Mathf.RoundToInt(row.ThirstDeltaPerSecond * Time.deltaTime));
        _guestStates.AddFatigue(Mathf.RoundToInt(row.FatigueDeltaPerSecond * Time.deltaTime));
        _guestStates.AddCleanliness(Mathf.RoundToInt(row.CleanlinessDeltaPerSecond * Time.deltaTime));
        _guestStates.AddSatisfaction(Mathf.RoundToInt(row.SatisfactionDeltaPerSecond * Time.deltaTime));
    }

    /// <summary>
    /// Utility AI 실행
    /// 현재 가장 급한 욕구와 목표 시설 타입을 계산
    /// </summary>
    public void EvaluateCurrentNeed()
    {
        if (_utilityEvaluator == null)
        {
            Debug.LogError("[GuestController] UtilityEvaluator is missing.");
            return;
        }

        CurrentNeedType = _utilityEvaluator.EvaluateHighestNeed(_guestStates);
        CurrentTargetFacilityType = _utilityEvaluator.EvaluateTargetFacilityType(_guestStates);

        Debug.Log($"[GuestController] EvaluateCurrentNeed | Need = {CurrentNeedType}, TargetFacilityType = {CurrentTargetFacilityType}");
    }

    /// <summary>
    /// 현재 목표 시설 ID 설정
    /// 나중에 시설 담당자 코드와 연결 예정
    /// </summary>
    public void SetCurrentTargetFacilityID(int facilityID)
    {
        CurrentTargetFacilityID = facilityID;
        Debug.Log($"[GuestController] CurrentTargetFacilityID set to {facilityID}");
    }

    /// <summary>
    /// 이동 완료 처리
    /// 나중에 A* 담당자 코드에서 호출 예정
    /// </summary>
    public void SetArrivedAtFacility(bool isArrived)
    {
        HasArrivedAtFacility = isArrived;
        Debug.Log($"[GuestController] HasArrivedAtFacility = {HasArrivedAtFacility}");
    }

    /// <summary>
    /// 시설 사용 가능 여부 설정
    /// 나중에 시설 담당자 코드에서 호출 예정
    /// </summary>
    public void SetCanUseFacility(bool canUse)
    {
        CanUseFacility = canUse;
        Debug.Log($"[GuestController] CanUseFacility = {CanUseFacility}");
    }

    /// <summary>
    /// 현재 시설 효과 적용
    /// GuestUseState에서 호출 예정
    /// </summary>
    public void ApplyCurrentFacilityEffect()
    {
        if (_facilityEffectDatabase == null)
        {
            Debug.LogError("[GuestController] FacilityEffectDatabase is missing.");
            return;
        }

        if (CurrentTargetFacilityID < 0)
        {
            Debug.LogWarning("[GuestController] CurrentTargetFacilityID is invalid.");
            return;
        }

        FacilityEffectRow effectRow = _facilityEffectDatabase.GetEffectByFacilityID(CurrentTargetFacilityID);

        if (effectRow == null)
        {
            Debug.LogWarning($"[GuestController] EffectRow not found. FacilityID: {CurrentTargetFacilityID}");
            return;
        }

        _guestStates.ApplyFacilityEffect(effectRow);
    }

    // -----------------------------
    // FSM 상태 전환 함수들
    // -----------------------------

    public void ChangeToIdleState()
    {
        _stateMachine.ChangeState(_idleState);
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

    // -----------------------------
    // 디버그용 ContextMenu
    // -----------------------------

    [ContextMenu("Debug Arrive Facility")]
    private void DebugArriveFacility()
    {
        SetArrivedAtFacility(true);
    }

    [ContextMenu("Debug Can Use Facility")]
    private void DebugCanUseFacility()
    {
        SetCanUseFacility(true);
    }

    [ContextMenu("Debug Apply Current Facility Effect")]
    private void DebugApplyCurrentFacilityEffect()
    {
        ApplyCurrentFacilityEffect();
    }
}