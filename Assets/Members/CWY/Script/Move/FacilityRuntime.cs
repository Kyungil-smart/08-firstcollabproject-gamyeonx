using UnityEngine;

public class FacilityRuntime : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private int _facilityID;
    [SerializeField] private EFacilityType _facilityType;

    [Header("외부 입구 정보")]
    [Tooltip("손님이 A*로 찾아갈 입구 앞 Road")]
    [SerializeField] private Vector3Int _entranceRoadCell;

    [Header("내부 포인트")]
    [SerializeField] private Transform _interiorEntryPoint;
    [SerializeField] private Transform _waitPoint;
    [SerializeField] private Transform _usePoint;
    [SerializeField] private Transform _outsideExitPoint;

    [Header("시설 상태")]
    [SerializeField] private bool _canUseImmediately = true;
    [SerializeField] private bool _supportsQueue = true;

    public int FacilityID => _facilityID;
    public EFacilityType FacilityType => _facilityType;
    public Vector3Int EntranceRoadCell => _entranceRoadCell;

    public Transform InteriorEntryPoint => _interiorEntryPoint;
    public Transform WaitPoint => _waitPoint;
    public Transform UsePoint => _usePoint;
    public Transform OutsideExitPoint => _outsideExitPoint;

    public bool CanUseImmediately => _canUseImmediately;
    public bool SupportsQueue => _supportsQueue;
}