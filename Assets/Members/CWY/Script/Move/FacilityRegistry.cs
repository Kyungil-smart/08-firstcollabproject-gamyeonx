using System.Collections.Generic;
using UnityEngine;

public class FacilityRegistry : MonoBehaviour
{
    public static FacilityRegistry Instance { get; private set; }

    [SerializeField] private List<FacilityRuntime> _facilityList = new List<FacilityRuntime>();

    private Dictionary<int, FacilityRuntime> _facilityMap = new Dictionary<int, FacilityRuntime>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
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

        for(int i = 0; i < _facilityList.Count; i++)
        {
            FacilityRuntime facility = _facilityList[i];

            if(facility == null)
            {
                continue;
            }

            _facilityMap[facility.FacilityID] = facility;
        }
    }

    public FacilityRuntime GetFacility(int facilityID)
    {
        if(_facilityMap.TryGetValue(facilityID, out FacilityRuntime facility))
        {
            return facility;
        }

        return null;
    }
    
    public void RegisterFacility(FacilityRuntime facility)
    {
        if(facility == null) return;
        _facilityMap[facility.FacilityID] = facility;
    
        if(!_facilityList.Contains(facility))
            _facilityList.Add(facility);
    }
}