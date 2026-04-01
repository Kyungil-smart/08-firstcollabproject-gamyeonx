using System.Collections.Generic;
using UnityEngine;

public class FacilityRuntime : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private int _facilityID;
    [SerializeField] public EFacilityType _facilityType;

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

    public InBuildingData _inBuildingData;

    [Header("시설 이용 가격")]
    public int Gold;

    private readonly Dictionary<Transform, GuestController> _slotUsers = new Dictionary<Transform, GuestController>();
    private readonly Dictionary<GuestController, Transform> _guestAssignedSlots = new Dictionary<GuestController, Transform>();
    private readonly Queue<GuestController> _waitQueue = new Queue<GuestController>();

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

        if (_inBuildingData.EntranceWayPivots != null)
        {
            foreach (var way in _inBuildingData.EntranceWayPivots)
            {
                _entranceWayPoints.Add(way.transform);
            }
        }

        if (_inBuildingData.ExitWayPivots != null)
        {
            foreach (var way in _inBuildingData.ExitWayPivots)
            {
                _exitWayPoints.Add(way.transform);
            }
        }

        HandleUsePivotsChanged(_inBuildingData.GetUsePivots());

        Debug.Log($"[FacilityRuntime] 초기화 완료 | FacilityID={_facilityID}, UseSlotCount={_usePoints.Count}");
    }

    private void OnDisable()
    {
        if (_inBuildingData != null)
        {
            _inBuildingData.OnUsePivotsChanged -= HandleUsePivotsChanged;
        }
    }

    private void HandleUsePivotsChanged(List<Transform> pivots)
    {
        _usePoints.Clear();

        if (pivots != null)
        {
            _usePoints.AddRange(pivots);
        }

        CleanupInvalidSlotData();

        Debug.Log($"[FacilityRuntime] 현재 좌석 수: {_usePoints.Count}");
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

    public int GetPrice()
    {
        return Gold;
    }

    public void UpgradePrice(int addPrice)
    {
        Gold += addPrice;
    }

    public void DownGradePrice(int minPrice)
    {
        Gold -= minPrice;
    }

    public int FacilityID => _facilityID;
    public EFacilityType FacilityType => _facilityType;

    public Vector3Int EntranceRoadCell
    {
        get
        {
            if (_entranceRoadObject == null)
            {
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

    public Transform EnterancePoint => _entrancePoint;
    
    public Transform ExitPoint => _exitPoint;

    public List<Transform> EnteranceWayPoints => _entranceWayPoints;
    
    public List<Transform> ExitWayPoints => _exitWayPoints;

    public bool CanUseImmediately => _canUseImmediately;
    public bool SupportsQueue => _supportsQueue;

    public int CurrentUsingGuestCount => _slotUsers.Count;
    public int CurrentWaitingGuestCount => _waitQueue.Count;
}