using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FacilityEffectDatabase", menuName = "Game/Facility/Facility Effect Database")]
public class FacilityEffectDatabaseSO : ScriptableObject
{
    [SerializeField] private List<FacilityEffectRow> _effectRowList = new List<FacilityEffectRow>();

    public IReadOnlyList<FacilityEffectRow> EffectRowList => _effectRowList;

    public event Action OnDatabaseChanged;

    public void Clear()
    {
        _effectRowList.Clear();
    }

    public void ReplaceAll(List<FacilityEffectRow> newRows)
    {
        _effectRowList.Clear();

        if (newRows != null)
        {
            _effectRowList.AddRange(newRows);
        }

        Debug.Log($"[FacilityEffectDatabaseSO] 데이터 갱신 완료 | Count={_effectRowList.Count}");
        OnDatabaseChanged?.Invoke();
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

    public int GetUsageFeeByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);
        return row != null ? row.UsageFee : 0;
    }

    public int GetBuildCostByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);
        return row != null ? row.BuildCost : 0;
    }

    public int GetUpgradeCostByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);
        return row != null ? row.UpgradeCost : 0;
    }

    public int GetRefundAmountByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);
        return row != null ? row.RefundAmount : 0;
    }

    public int GetUnlockRevenueByFacilityID(string facilityID)
    {
        FacilityEffectRow row = GetEffectByFacilityID(facilityID);
        return row != null ? row.UnlockRevenue : 0;
    }
}