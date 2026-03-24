using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FacilityEffectDatabase", menuName = "Game/Facility/Facility Effect Database")]
public class FacilityEffectDatabaseSO : ScriptableObject
{
    [Header("Facility Effect Row List")]
    // 모든 시설 효과 데이터 목록
    [SerializeField] private List<FacilityEffectRow> _effectRowList = new List<FacilityEffectRow>();

    public IReadOnlyList<FacilityEffectRow> EffectRowList => _effectRowList;

    public FacilityEffectRow GetEffectByFacilityID(int facilityID)
    {
        for (int i = 0; i < _effectRowList.Count; i++)
        {
            FacilityEffectRow row = _effectRowList[i];

            if (row == null)
            {
                continue;
            }

            if (row.FacilityID == facilityID)
            {
                return row;
            }
        }

        Debug.LogWarning($"[FacilityEffectDatabaseSO] Effect not found. FacilityID: {facilityID}");
        return null;
    }

    public FacilityEffectRow GetEffectByFacilityType(EFacilityType facilityType)
    {
        for (int i = 0; i < _effectRowList.Count; i++)
        {
            FacilityEffectRow row = _effectRowList[i];

            if (row == null)
            {
                continue;
            }

            if (row.EFacilityType == facilityType)
            {
                return row;
            }
        }

        Debug.LogWarning($"[FacilityEffectDatabaseSO] Effect not found. EFacilityType: {facilityType}");
        return null;
    }

    public void Clear()
    {
        _effectRowList.Clear();
    }

    public void AddEffectRow(FacilityEffectRow row)
    {
        if (row == null)
        {
            Debug.LogWarning("[FacilityEffectDatabaseSO] AddEffectRow failed. Row is null.");
            return;
        }

        _effectRowList.Add(row);
    }

    public bool ContainsFacilityID(int facilityID)
    {
        for (int i = 0; i < _effectRowList.Count; i++)
        {
            FacilityEffectRow row = _effectRowList[i];

            if (row == null)
            {
                continue;
            }

            if (row.FacilityID == facilityID)
            {
                return true;
            }
        }

        return false;
    }
}