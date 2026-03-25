using UnityEngine;

/// <summary>
/// Main controller for one guest.
/// This class connects guest data, runtime states, utility AI, and FSM states.
/// </summary>
public class GuestController : MonoBehaviour
{
    [Header("Guest Identity")]
    [SerializeField] private int _visitorID = 1;

    [Header("Database References")]
    [SerializeField] private GuestDataDatabaseSO _guestDataDatabase;
    [SerializeField] private FacilityEffectDatabaseSO _facilityEffectDatabase;

    [Header("Runtime Guest States")]
    [SerializeField] private GuestStates _guestStates = new GuestStates();

    [Header("State Durations")]
    [SerializeField] private float _idleDuration = 2f;

    [Header("Use Goal Settings")]
    [Tooltip("How often facility effect is applied while using.")]
    [SerializeField] private float _useEffectTickInterval = 1f;

    [Tooltip("Food use ends when hunger is equal to or below this value.")]
    [SerializeField, Range(0, 100)] private int _hungerExitThreshold = 25;

    [Tooltip("Drink use ends when thirst is equal to or below this value.")]
    [SerializeField, Range(0, 100)] private int _thirstExitThreshold = 25;

    [Tooltip("Rest use ends when fatigue is equal to or below this value.")]
    [SerializeField, Range(0, 100)] private int _fatigueExitThreshold = 25;

    [Tooltip("Clean use ends when cleanliness is equal to or above this value.")]
    [SerializeField, Range(0, 100)] private int _cleanlinessExitThreshold = 75;

    public GuestStates GuestStates => _guestStates;
    public float IdleDuration => _idleDuration;
    public float WanderDuration { get; private set; } = 3f;
    public float UseEffectTickInterval => _useEffectTickInterval;

    public EGuestNeedType CurrentNeedType { get; private set; } = EGuestNeedType.None;
    public EFacilityType CurrentTargetFacilityType { get; private set; } = EFacilityType.None;

    public bool HasArrivedAtFacility { get; private set; }
    public bool CanUseFacility { get; private set; }
    public int CurrentTargetFacilityID { get; private set; } = -1;

    private GuestUtilityEvaluator _utilityEvaluator;
    private GuestStateMachine _stateMachine;

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
        _stateMachine?.Update();
        UpdateGuestStatesOverTime();
    }

    /// <summary>
    /// Initialize runtime systems and FSM states.
    /// </summary>
    private void Initialize()
    {
        _utilityEvaluator = new GuestUtilityEvaluator();
        _stateMachine = new GuestStateMachine();

        _idleState = new GuestIdleState(this);
        _wanderState = new GuestWanderState(this);
        _decideState = new GuestDecideState(this);
        _moveState = new GuestMoveState(this);
        _waitState = new GuestWaitState(this);
        _useState = new GuestUseState(this);

        LoadGuestData();
        _stateMachine.ChangeState(_idleState);

        Debug.Log("[GuestController] Initialized.");
    }

    /// <summary>
    /// Load initial guest values from database by visitor ID.
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

        _guestStates.Initialize(
            row.VisitorID,
            row.Hunger,
            row.Thirst,
            row.Fatigue,
            row.Cleanliness,
            row.Satisfaction
        );

        WanderDuration = row.WanderDuration;

        Debug.Log($"[GuestController] Guest data loaded. VisitorID: {_visitorID}");
    }

    /// <summary>
    /// Apply passive stat change over time.
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

        _guestStates.AddHunger(Mathf.RoundToInt(row.HungerDeltaPerSecond * Time.deltaTime));
        _guestStates.AddThirst(Mathf.RoundToInt(row.ThirstDeltaPerSecond * Time.deltaTime));
        _guestStates.AddFatigue(Mathf.RoundToInt(row.FatigueDeltaPerSecond * Time.deltaTime));
        _guestStates.AddCleanliness(Mathf.RoundToInt(row.CleanlinessDeltaPerSecond * Time.deltaTime));
        _guestStates.AddSatisfaction(Mathf.RoundToInt(row.SatisfactionDeltaPerSecond * Time.deltaTime));
    }

    /// <summary>
    /// Evaluate current highest need and target facility type.
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
    /// Set current target facility ID.
    /// </summary>
    public void SetCurrentTargetFacilityID(int facilityID)
    {
        CurrentTargetFacilityID = facilityID;
        Debug.Log($"[GuestController] CurrentTargetFacilityID set to {facilityID}");
    }

    /// <summary>
    /// Set arrival state from movement system.
    /// </summary>
    public void SetArrivedAtFacility(bool isArrived)
    {
        HasArrivedAtFacility = isArrived;
        Debug.Log($"[GuestController] HasArrivedAtFacility = {HasArrivedAtFacility}");
    }

    /// <summary>
    /// Set availability from facility system.
    /// </summary>
    public void SetCanUseFacility(bool canUse)
    {
        CanUseFacility = canUse;
        Debug.Log($"[GuestController] CanUseFacility = {CanUseFacility}");
    }

    /// <summary>
    /// Apply current facility effect once.
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

    /// <summary>
    /// Return true when the current facility's main goal is already satisfied.
    /// </summary>
    public bool IsCurrentFacilityGoalReached()
    {
        switch (CurrentTargetFacilityType)
        {
            case EFacilityType.Food:
                return _guestStates.hunger <= _hungerExitThreshold;

            case EFacilityType.Drink:
                return _guestStates.thirst <= _thirstExitThreshold;

            case EFacilityType.Rest:
                return _guestStates.fatigue <= _fatigueExitThreshold;

            case EFacilityType.Clean:
                return _guestStates.cleanliness >= _cleanlinessExitThreshold;

            default:
                Debug.LogWarning("[GuestController] IsCurrentFacilityGoalReached failed. Invalid facility type.");
                return true;
        }
    }

    /// <summary>
    /// Clear runtime facility context after use ends.
    /// </summary>
    public void ClearCurrentFacilityContext()
    {
        CurrentTargetFacilityID = -1;
        CurrentTargetFacilityType = EFacilityType.None;
        HasArrivedAtFacility = false;
        CanUseFacility = false;

        Debug.Log("[GuestController] Cleared current facility context.");
    }

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