using System;
using System.Collections.Generic;
using UnityEngine;

public class FacilityRuntime : MonoBehaviour
{
    [Header("�⺻ ����")]
    [SerializeField] private int _facilityID;
    [SerializeField] public EFacilityType _facilityType;

    [Header("�ܺ� �Ա� ����")]
    [Tooltip("�մ��� A*�� ã�ư� �Ա� �� Road")]
    [SerializeField] private GameObject _entranceRoadObject;

    [Header("���� ����Ʈ")]
    [SerializeField] private Transform _interiorEntryPoint;
    [SerializeField] private Transform _waitPoint;
    [SerializeField] private Transform _usePoint;
    [SerializeField] private List<Transform> _usePoints = new List<Transform>();
    [SerializeField] private Transform _outsideExitPoint;
    [SerializeField] private Transform _facilityExitPoint;

    [SerializeField] private Transform _entrancePoint;
    [SerializeField] private Transform _exitPoint;
    [SerializeField] private List<Transform> _entranceWayPoints = new List<Transform>();
    [SerializeField] private List<Transform> _exitWayPoints = new List<Transform>();
    
    [Header("�ü� ����")]
    [SerializeField] private bool _canUseImmediately = true;
    [SerializeField] private bool _supportsQueue = true;
    
    public InBuildingData _inBuildingData;
    
    //연동준이 추가
    [Header("시설 이용 가격")] 
    public int Gold;


    void Start()
    {
        // OnEnable 시점에서는 inBuildingData가 없는 것 같음 그래서 Start에서 구독
        _inBuildingData.OnUsePivotsChanged += HandleUsePivotsChanged; 
        
        _interiorEntryPoint = _inBuildingData.EnterPivot.transform;
        _waitPoint = _inBuildingData.WaitPivot.transform;
        _usePoint = _inBuildingData.UsePivot.transform;
        
        _entrancePoint = _inBuildingData.EntrancePivot.transform;
        _exitPoint = _inBuildingData.ExitPivot.transform;
        _facilityExitPoint = _inBuildingData.FacilityExitPivot.transform;
        
        HandleUsePivotsChanged(_inBuildingData.GetUsePivots());
        
        foreach (var way in _inBuildingData.EntranceWayPivots)
        {
            _entranceWayPoints.Add(way.transform);
        }
    
        foreach (var way in _inBuildingData.ExitWayPivots)
        {
            _exitWayPoints.Add(way.transform);
        }
    }

    void OnDisable()
    { 
        _inBuildingData.OnUsePivotsChanged -= HandleUsePivotsChanged;
    }

    // 시설 이용 가능 공간 개수가 바뀔 때마다 _usePoints 재반영
    private void HandleUsePivotsChanged(List<Transform> pivots)
    {
        _usePoints.Clear();
        _usePoints.AddRange(pivots);

        Debug.Log($"현재 좌석 수: {_usePoints.Count}");
    }
    
    // private void Update()
    // {
    //     _interiorEntryPoint = _inBuildingData.EnterPivot.transform;
    //     _waitPoint = _inBuildingData.WaitPivot.transform;
    //     _usePoint = _inBuildingData.UsePivot.transform;
    //     
    //     _entrancePoint = _inBuildingData.EntrancePivot.transform;
    //     _exitPoint = _inBuildingData.ExitPivot.transform;
    //     
    //     foreach (var usePivot in _inBuildingData.UsePivots)
    //     {
    //         _usePoints.Add(usePivot.transform);
    //     }
    //     
    //     foreach (var way in _inBuildingData.EntranceWayPivots)
    //     {
    //         _entranceWayPoints.Add(way.transform);
    //     }
    //
    //     foreach (var way in _inBuildingData.ExitWayPivots)
    //     {
    //         _exitWayPoints.Add(way.transform);
    //     }
    // }
    
    public int GetPrice() // 시설 이용 가격을 알려주는 메서드
    {
        return Gold;
    }

    public void UpgradePrice(int addPrice) // 시설 2번째 업그레이드 시 가격 인상 메서드
    {
        Gold += addPrice;
    }
    
    public int FacilityID => _facilityID;
    public EFacilityType FacilityType => _facilityType;
    public Vector3Int EntranceRoadCell
    {
        get
        {
            if (_entranceRoadObject == null) return Vector3Int.zero;
            return GridBuildingSystem.Instance.gridLayout.WorldToCell(
                _entranceRoadObject.transform.position);
        }
    }

    public Transform InteriorEntryPoint => _interiorEntryPoint;
    public Transform WaitPoint => _waitPoint;
    public Transform UsePoint => _usePoint;
    // public List<Transform> UsePoint => _usePoint;
    public Transform OutsideExitPoint => _outsideExitPoint;

    public bool CanUseImmediately => _canUseImmediately;
    public bool SupportsQueue => _supportsQueue;
}