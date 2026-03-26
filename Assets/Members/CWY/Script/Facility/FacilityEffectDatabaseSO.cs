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

            if (row.FacilityType == facilityType && row.NormalGuest)
            {
                return row;
            }
        }

        return null;
    }
}