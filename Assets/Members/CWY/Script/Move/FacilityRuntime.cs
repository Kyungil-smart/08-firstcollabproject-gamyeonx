using System;
using UnityEngine;

public class FacilityRuntime : MonoBehaviour
{
    [Header("�⺻ ����")]
    [SerializeField] private int _facilityID;
    [SerializeField] private EFacilityType _facilityType;

    [Header("�ܺ� �Ա� ����")]
    [Tooltip("�մ��� A*�� ã�ư� �Ա� �� Road")]
    [SerializeField] private GameObject _entranceRoadObject;

    [Header("���� ����Ʈ")]
    [SerializeField] private Transform _interiorEntryPoint;
    [SerializeField] private Transform _waitPoint;
    [SerializeField] private Transform _usePoint;
    [SerializeField] private Transform _outsideExitPoint;

    [Header("�ü� ����")]
    [SerializeField] private bool _canUseImmediately = true;
    [SerializeField] private bool _supportsQueue = true;
    
    public InBuildingData _inBuildingData;

    private void Update()
    {
        _interiorEntryPoint = _inBuildingData.EnterPivot.transform;
        _waitPoint = _inBuildingData.WaitPivot.transform;
        _usePoint = _inBuildingData.UsePivot.transform;
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
    public Transform OutsideExitPoint => _outsideExitPoint;

    public bool CanUseImmediately => _canUseImmediately;
    public bool SupportsQueue => _supportsQueue;
}