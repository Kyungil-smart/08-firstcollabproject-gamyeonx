using System.Collections.Generic;
using UnityEngine;

public class FacilityRuntime : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private string _facilityID;
    [SerializeField] public EFacilityType _facilityType;

    [Header("시설 데이터")]
    [SerializeField] private FacilityEffectDatabaseSO _facilityEffectDatabase;

    [Header("런타임 데이터")]
    [SerializeField] private FacilityRuntimeData _runtimeData = new FacilityRuntimeData();

    [Header("외부 입구 정보")]
    [Tooltip("손님이 A*로 찾아올 입구 앞 Road")]
    [SerializeField] private GameObject _entranceRoadObject;

    [Header("내부 포인트")]
    [SerializeField] private Transform _interiorEntryPoint;
    [SerializeField] private Transform _waitPoint;
    [SerializeField] private List<Transform> _usePoints = new List<Transform>();
    [SerializeField] private Transform _facilityExitPoint;
    [SerializeField] private Transform _outsideExitPoint;
    [SerializeField] private Transform _entrancePoint;
    [SerializeField] private Transform _exitPoint;
    [SerializeField] private List<Transform> _entranceWayPoints = new List<Transform>();
    [SerializeField] private List<Transform> _exitWayPoints = new List<Transform>();
    
    [Header("시설 설정")]
    [SerializeField] private bool _canUseImmediately = true;
    [SerializeField] private bool _supportsQueue = true;

    [Header("건물 내부 데이터")]
    public InBuildingData _inBuildingData;

    private readonly Dictionary<Transform, GuestController> _slotUsers = new Dictionary<Transform, GuestController>();
    private readonly Dictionary<GuestController, Transform> _guestAssignedSlots = new Dictionary<GuestController, Transform>();
    private readonly Queue<GuestController> _waitQueue = new Queue<GuestController>();

    public int FurnitureGold = 0;

    public int TotalPay()
    {
        return FurnitureGold;
    }
    
    
    public string FacilityID => _facilityID;
    public EFacilityType FacilityType => _facilityType;

    public FacilityRuntimeData RuntimeData => _runtimeData;

    public string FacilityNameKo => _runtimeData.FacilityNameKo;
    public string FacilityNameEn => _runtimeData.FacilityNameEn;

    public int RefundAmount => _runtimeData.RefundAmount;
    public int BuildCost => _runtimeData.BuildCost;
    public int UpgradeCost => _runtimeData.UpgradeCost;
    public int UnlockRevenue => _runtimeData.UnlockRevenue;
    public int UsageFee => _runtimeData.UsageFee;

    public int FatigueEffectPerTick => _runtimeData.FatigueEffectPerTick;
    public int ThirstEffectPerTick => _runtimeData.ThirstEffectPerTick;
    public int HungerEffectPerTick => _runtimeData.HungerEffectPerTick;
    public int ShopEffectPerTick => _runtimeData.ShopEffectPerTick;
    public int TrainingEffectPerTick => _runtimeData.TrainingEffectPerTick;

    // 기존 다른 코드 호환용
    public InBuildingData inBuildingData => _inBuildingData;

    public Vector3Int EntranceRoadCell
    {
        get
        {
            if (_entranceRoadObject == null)
            {
                return Vector3Int.zero;
            }

            if (GridBuildingSystem.Instance == null || GridBuildingSystem.Instance.gridLayout == null)
            {
                Debug.LogWarning("[FacilityRuntime] GridBuildingSystem 또는 gridLayout이 없습니다.");
                return Vector3Int.zero;
            }

            return GridBuildingSystem.Instance.gridLayout.WorldToCell(_entranceRoadObject.transform.position);
        }
    }

    public Transform InteriorEntryPoint => _interiorEntryPoint;
    public Transform WaitPoint => _waitPoint;
    public List<Transform> UsePoints => _usePoints;
    public Transform FacilityExitPoint => _facilityExitPoint;
    public Transform OutsideExitPoint => _outsideExitPoint;

    public bool CanUseImmediately => _canUseImmediately;
    public bool SupportsQueue => _supportsQueue;

    public int CurrentUsingGuestCount => _slotUsers.Count;
    public int CurrentWaitingGuestCount => _waitQueue.Count;

    private void Awake()
    {
        SubscribeDatabase();
        SyncFromFacilityRow();

        if (FacilityRegistry.Instance != null)
        {
            FacilityRegistry.Instance.RegisterFacility(this);
        }
    }

    private void Start()
    {
        if (_inBuildingData == null)
        {
            Debug.LogWarning($"[FacilityRuntime] InBuildingData가 비어 있습니다. name={name}");
            return;
        }

        _inBuildingData.OnUsePivotsChanged += HandleUsePivotsChanged;

        if (_inBuildingData.EnterPivot != null)
        {
            _interiorEntryPoint = _inBuildingData.EnterPivot.transform;
        }

        if (_inBuildingData.WaitPivot != null)
        {
            _waitPoint = _inBuildingData.WaitPivot.transform;
        }

        if (_inBuildingData.FacilityExitPivot != null)
        {
            _facilityExitPoint = _inBuildingData.FacilityExitPivot.transform;
        }

        if (_inBuildingData.EntrancePivot != null)
        {
            _entrancePoint = _inBuildingData.EntrancePivot.transform;
        }

        if (_inBuildingData.ExitPivot != null)
        {
            _exitPoint = _inBuildingData.ExitPivot.transform;
        }

        _entranceWayPoints.Clear();
        if (_inBuildingData.EntranceWayPivots != null)
        {
            foreach (var way in _inBuildingData.EntranceWayPivots)
            {
                if (way != null)
                {
                    _entranceWayPoints.Add(way.transform);
                }
            }
        }

        _exitWayPoints.Clear();
        if (_inBuildingData.ExitWayPivots != null)
        {
            foreach (var way in _inBuildingData.ExitWayPivots)
            {
                if (way != null)
                {
                    _exitWayPoints.Add(way.transform);
                }
            }
        }

        HandleUsePivotsChanged(_inBuildingData.GetUsePivots());

        Debug.Log($"[FacilityRuntime] 초기화 완료 | FacilityID={_facilityID}, FacilityType={_facilityType}, UsageFee={UsageFee}");
    }

    private void OnDisable()
    {
        UnsubscribeDatabase();

        if (_inBuildingData != null)
        {
            _inBuildingData.OnUsePivotsChanged -= HandleUsePivotsChanged;
        }

        if (FacilityRegistry.Instance != null)
        {
            FacilityRegistry.Instance.UnregisterFacility(this);
        }
    }

    public void InitializeFacility(string facilityID)
    {
        if (string.IsNullOrWhiteSpace(facilityID))
        {
            Debug.LogWarning("[FacilityRuntime] InitializeFacility 실패 - facilityID가 비어 있습니다.");
            return;
        }

        string oldFacilityID = _facilityID;

        if (FacilityRegistry.Instance != null && string.IsNullOrWhiteSpace(oldFacilityID) == false)
        {
            FacilityRegistry.Instance.UnregisterFacilityByID(oldFacilityID, this);
        }

        _facilityID = facilityID;
        SyncFromFacilityRow();

        if (FacilityRegistry.Instance != null)
        {
            FacilityRegistry.Instance.RegisterFacility(this);
        }

        Debug.Log($"[FacilityRuntime] 시설 초기화 완료 | OldID={oldFacilityID}, NewID={_facilityID}, FacilityType={_facilityType}, UsageFee={UsageFee}");
    }

    private void SubscribeDatabase()
    {
        if (_facilityEffectDatabase == null)
        {
            return;
        }

        _facilityEffectDatabase.OnDatabaseChanged -= HandleDatabaseChanged;
        _facilityEffectDatabase.OnDatabaseChanged += HandleDatabaseChanged;
    }

    private void UnsubscribeDatabase()
    {
        if (_facilityEffectDatabase == null)
        {
            return;
        }

        _facilityEffectDatabase.OnDatabaseChanged -= HandleDatabaseChanged;
    }

    private void HandleDatabaseChanged()
    {
        SyncFromFacilityRow();
    }

    private void SyncFromFacilityRow()
    {
        if (_facilityEffectDatabase == null)
        {
            Debug.LogWarning($"[FacilityRuntime] FacilityEffectDatabaseSO가 없습니다. FacilityID={_facilityID}");
            return;
        }

        if (string.IsNullOrWhiteSpace(_facilityID))
        {
            Debug.LogWarning("[FacilityRuntime] FacilityID가 비어 있습니다.");
            return;
        }

        FacilityEffectRow row = _facilityEffectDatabase.GetEffectByFacilityID(_facilityID);

        if (row == null)
        {
            Debug.LogWarning($"[FacilityRuntime] FacilityEffectRow를 찾지 못했습니다. FacilityID={_facilityID}");
            return;
        }

        _runtimeData.ApplyRow(row);
        _facilityType = row.FacilityType;

        Debug.Log($"[FacilityRuntime] 시트 동기화 완료 | ID={_facilityID}, Refund={RefundAmount}, Build={BuildCost}, Upgrade={UpgradeCost}, Unlock={UnlockRevenue}, UsageFee={UsageFee}");
    }

    private void HandleUsePivotsChanged(List<Transform> pivots)
    {
        _usePoints.Clear();

        if (pivots != null)
        {
            _usePoints.AddRange(pivots);
        }

        CleanupInvalidSlotData();

        Debug.Log($"[FacilityRuntime] 현재 좌석 수 | FacilityID={_facilityID}, Count={_usePoints.Count}");
    }

    private void CleanupInvalidSlotData()
    {
        List<Transform> removeSlots = new List<Transform>();

        foreach (var pair in _slotUsers)
        {
            if (pair.Key == null || !_usePoints.Contains(pair.Key))
            {
                removeSlots.Add(pair.Key);
            }
        }

        foreach (Transform slot in removeSlots)
        {
            _slotUsers.Remove(slot);
        }

        List<GuestController> removeGuests = new List<GuestController>();

        foreach (var pair in _guestAssignedSlots)
        {
            if (pair.Key == null || pair.Value == null || !_usePoints.Contains(pair.Value))
            {
                removeGuests.Add(pair.Key);
            }
        }

        foreach (GuestController guest in removeGuests)
        {
            _guestAssignedSlots.Remove(guest);
        }
    }

    public bool TryRequestUse(GuestController guest, out Transform assignedUsePoint, out bool isQueued)
    {
        assignedUsePoint = null;
        isQueued = false;

        if (guest == null)
        {
            Debug.LogWarning("[FacilityRuntime] TryRequestUse 실패 - guest가 null입니다.");
            return false;
        }

        if (_guestAssignedSlots.TryGetValue(guest, out Transform existingSlot))
        {
            assignedUsePoint = existingSlot;
            return true;
        }

        Transform emptySlot = GetEmptyUseSlot();

        if (_canUseImmediately && emptySlot != null)
        {
            AssignSlot(guest, emptySlot);
            assignedUsePoint = emptySlot;

            Debug.Log($"[FacilityRuntime] 즉시 이용 배정 | Guest={guest.name}, Slot={emptySlot.name}");
            return true;
        }

        if (_supportsQueue)
        {
            if (!IsGuestAlreadyQueued(guest))
            {
                _waitQueue.Enqueue(guest);
                Debug.Log($"[FacilityRuntime] 대기열 등록 | Guest={guest.name}, QueueCount={_waitQueue.Count}");
            }

            isQueued = true;
            return true;
        }

        Debug.Log($"[FacilityRuntime] 이용 실패 | Guest={guest.name}");
        return false;
    }

    private Transform GetEmptyUseSlot()
    {
        for (int i = 0; i < _usePoints.Count; i++)
        {
            Transform slot = _usePoints[i];

            if (slot == null)
            {
                continue;
            }

            if (_slotUsers.ContainsKey(slot) == false)
            {
                return slot;
            }
        }

        return null;
    }

    private void AssignSlot(GuestController guest, Transform slot)
    {
        if (guest == null || slot == null)
        {
            return;
        }

        _slotUsers[slot] = guest;
        _guestAssignedSlots[guest] = slot;
    }

    private bool IsGuestAlreadyQueued(GuestController guest)
    {
        foreach (GuestController queuedGuest in _waitQueue)
        {
            if (queuedGuest == guest)
            {
                return true;
            }
        }

        return false;
    }

    public void ReleaseGuest(GuestController guest)
    {
        if (guest == null)
        {
            return;
        }

        bool releasedSlot = false;

        if (_guestAssignedSlots.TryGetValue(guest, out Transform assignedSlot))
        {
            _guestAssignedSlots.Remove(guest);

            if (_slotUsers.ContainsKey(assignedSlot))
            {
                _slotUsers.Remove(assignedSlot);
                releasedSlot = true;

                Debug.Log($"[FacilityRuntime] 좌석 반납 | Guest={guest.name}, Slot={assignedSlot.name}");
            }
        }

        RemoveGuestFromWaitQueue(guest);

        if (releasedSlot)
        {
            PromoteNextWaitingGuest();
        }
    }

    private void RemoveGuestFromWaitQueue(GuestController guest)
    {
        if (_waitQueue.Count == 0)
        {
            return;
        }

        Queue<GuestController> newQueue = new Queue<GuestController>();

        while (_waitQueue.Count > 0)
        {
            GuestController queuedGuest = _waitQueue.Dequeue();

            if (queuedGuest == null || queuedGuest == guest)
            {
                continue;
            }

            newQueue.Enqueue(queuedGuest);
        }

        while (newQueue.Count > 0)
        {
            _waitQueue.Enqueue(newQueue.Dequeue());
        }
    }

    private void PromoteNextWaitingGuest()
    {
        Transform emptySlot = GetEmptyUseSlot();

        while (emptySlot != null && _waitQueue.Count > 0)
        {
            GuestController nextGuest = _waitQueue.Dequeue();

            if (nextGuest == null)
            {
                emptySlot = GetEmptyUseSlot();
                continue;
            }

            AssignSlot(nextGuest, emptySlot);
            nextGuest.NotifyUseSlotAssigned(emptySlot);

            Debug.Log($"[FacilityRuntime] 대기 손님 입장 | Guest={nextGuest.name}, Slot={emptySlot.name}");

            emptySlot = GetEmptyUseSlot();
        }
    }

    // ------------------------------
    // 기존 테스트용 가격 조작 코드
    // 최종 구조에서는 SO 기준으로만 사용하므로 주석 처리
    // ------------------------------

    /*
    public int Gold;
    */

    /*
    public int GetPrice()
    {
        return Gold;
    }
    */

    /*
    public void UpgradePrice(int addPrice)
    {
        Gold += addPrice;
    }
    */

    /*
    public void DownGradePrice(int minPrice)
    {
        Gold -= minPrice;
    }
    */

}







