using System.Collections.Generic;
using UnityEngine;

public class FacilityRegistry : MonoBehaviour
{
    public static FacilityRegistry Instance { get; private set; }

    [SerializeField] private List<FacilityRuntime> _facilityList = new List<FacilityRuntime>();

    private readonly Dictionary<string, FacilityRuntime> _facilityMap = new Dictionary<string, FacilityRuntime>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildMap();
    }

    private void BuildMap()
    {
        _facilityMap.Clear();

        for (int i = 0; i < _facilityList.Count; i++)
        {
            FacilityRuntime facility = _facilityList[i];

            if (facility == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(facility.FacilityID))
            {
                Debug.LogWarning($"[FacilityRegistry] FacilityID가 비어 있는 시설이 있습니다. name={facility.name}");
                continue;
            }

            _facilityMap[facility.FacilityID] = facility;
        }
    }

    public FacilityRuntime GetFacility(string facilityID)
    {
        if (string.IsNullOrWhiteSpace(facilityID))
        {
            Debug.LogWarning("[FacilityRegistry] facilityID가 비어 있습니다.");
            return null;
        }

        if (_facilityMap.TryGetValue(facilityID, out FacilityRuntime facility))
        {
            return facility;
        }

        Debug.LogWarning($"[FacilityRegistry] 시설을 찾지 못했습니다. FacilityID={facilityID}");
        return null;
    }

    public void RegisterFacility(FacilityRuntime facility)
    {
        if (facility == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(facility.FacilityID))
        {
            Debug.LogWarning($"[FacilityRegistry] 등록 실패 - FacilityID가 비어 있습니다. name={facility.name}");
            return;
        }

        _facilityMap[facility.FacilityID] = facility;

        if (!_facilityList.Contains(facility))
        {
            _facilityList.Add(facility);
        }

        Debug.Log($"[FacilityRegistry] 시설 등록 | FacilityID={facility.FacilityID}");
    }

    public void UnregisterFacility(FacilityRuntime facility)
    {
        if (facility == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(facility.FacilityID))
        {
            return;
        }

        if (_facilityMap.ContainsKey(facility.FacilityID))
        {
            _facilityMap.Remove(facility.FacilityID);
        }

        if (_facilityList.Contains(facility))
        {
            _facilityList.Remove(facility);
        }

        Debug.Log($"[FacilityRegistry] 시설 해제 | FacilityID={facility.FacilityID}");
    }
}