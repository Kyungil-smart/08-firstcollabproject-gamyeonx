using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FacilityEffectDatabase", menuName = "Game/Facility/Facility Effect Database")]
public class FacilityEffectDatabaseSO : ScriptableObject
{
    [SerializeField] private List<FacilityEffectRow> _effectRowList = new List<FacilityEffectRow>();

    public IReadOnlyList<FacilityEffectRow> EffectRowList => _effectRowList;

    public void Clear()
    {
        _effectRowList.Clear();
    }

    public void AddEffectRow(FacilityEffectRow row)
    {
        if (row == null)
        {
            Debug.LogWarning("[FacilityEffectDatabaseSO] row가 null이라 추가하지 못했습니다.");
            return;
        }

        _effectRowList.Add(row);
    }

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

        Debug.LogWarning($"[FacilityEffectDatabaseSO] FacilityID={facilityID} 데이터를 찾지 못했습니다.");
        return null;
    }

    public FacilityEffectRow GetFirstSelectableEffectByType(EFacilityType facilityType)
    {
        for (int i = 0; i < _effectRowList.Count; i++)
        {
            FacilityEffectRow row = _effectRowList[i];

            if (row == null)
            {
                continue;
            }

            if (row.FacilityType == facilityType && row.IsSelectableByNormalGuest)
            {
                return row;
            }
        }

        Debug.LogWarning($"[FacilityEffectDatabaseSO] 일반 손님이 선택 가능한 시설이 없습니다. FacilityType={facilityType}");
        return null;
    }
}