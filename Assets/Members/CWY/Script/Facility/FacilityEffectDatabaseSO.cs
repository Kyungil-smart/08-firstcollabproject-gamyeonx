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
            Debug.LogWarning("[FacilityEffectDatabaseSO] null row는 추가할 수 없습니다.");
            return;
        }

        _effectRowList.Add(row);
    }

    public FacilityEffectRow GetEffectByFacilityID(string facilityID)
    {
        if (string.IsNullOrWhiteSpace(facilityID))
        {
            Debug.LogWarning("[FacilityEffectDatabaseSO] facilityID가 비어 있습니다.");
            return null;
        }

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

        Debug.LogWarning($"[FacilityEffectDatabaseSO] 해당 FacilityID를 찾지 못했습니다. ID={facilityID}");
        return null;
    }

    public FacilityEffectRow GetFirstMatchingEffectByType(EFacilityType facilityType)
    {
        for (int i = 0; i < _effectRowList.Count; i++)
        {
            FacilityEffectRow row = _effectRowList[i];

            if (row == null)
            {
                continue;
            }

            if (row.FacilityType == facilityType)
            {
                return row;
            }
        }

        Debug.LogWarning($"[FacilityEffectDatabaseSO] 해당 FacilityType을 찾지 못했습니다. Type={facilityType}");
        return null;
    }

    public List<FacilityEffectRow> GetAllEffectsByType(EFacilityType facilityType)
    {
        List<FacilityEffectRow> result = new List<FacilityEffectRow>();

        for (int i = 0; i < _effectRowList.Count; i++)
        {
            FacilityEffectRow row = _effectRowList[i];

            if (row == null)
            {
                continue;
            }

            if (row.FacilityType == facilityType)
            {
                result.Add(row);
            }
        }

        return result;
    }

    public int GetUsageFeeByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);

        if (row == null)
        {
            return 0;
        }

        return row.UsageFee;
    }

    public int GetBuildCostByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);

        if (row == null)
        {
            return 0;
        }

        return row.BuildCost;
    }

    public int GetUpgradeCostByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);

        if (row == null)
        {
            return 0;
        }

        return row.UpgradeCost;
    }

    public int GetRefundAmountByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);

        if (row == null)
        {
            return 0;
        }

        return row.RefundAmount;
    }

    public int GetUnlockRevenueByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);

        if (row == null)
        {
            return 0;
        }

        return row.UnlockRevenue;
    }

}

