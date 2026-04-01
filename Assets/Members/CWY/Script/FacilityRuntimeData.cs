using System;
using UnityEngine;

[Serializable]
public class FacilityRuntimeData
{
    [SerializeField] private string _facilityID;
    [SerializeField] private EFacilityType _facilityType;
    [SerializeField] private string _facilityNameKo;
    [SerializeField] private string _facilityNameEn;

    [SerializeField] private int _refundAmount;
    [SerializeField] private int _buildCost;
    [SerializeField] private int _upgradeCost;
    [SerializeField] private int _unlockRevenue;
    [SerializeField] private int _usageFee;

    [SerializeField] private int _fatigueEffectPerTick;
    [SerializeField] private int _thirstEffectPerTick;
    [SerializeField] private int _hungerEffectPerTick;
    [SerializeField] private int _shopEffectPerTick;
    [SerializeField] private int _trainingEffectPerTick;

    public string FacilityID => _facilityID;
    public EFacilityType FacilityType => _facilityType;
    public string FacilityNameKo => _facilityNameKo;
    public string FacilityNameEn => _facilityNameEn;

    public int RefundAmount => _refundAmount;
    public int BuildCost => _buildCost;
    public int UpgradeCost => _upgradeCost;
    public int UnlockRevenue => _unlockRevenue;
    public int UsageFee => _usageFee;

    public int FatigueEffectPerTick => _fatigueEffectPerTick;
    public int ThirstEffectPerTick => _thirstEffectPerTick;
    public int HungerEffectPerTick => _hungerEffectPerTick;
    public int ShopEffectPerTick => _shopEffectPerTick;
    public int TrainingEffectPerTick => _trainingEffectPerTick;

    public event Action OnDataChanged;

    public void ApplyRow(FacilityEffectRow row)
    {
        if (row == null)
        {
            Debug.LogWarning("[FacilityRuntimeData] row가 null입니다.");
            return;
        }

        _facilityID = row.FacilityID;
        _facilityType = row.FacilityType;
        _facilityNameKo = row.FacilityNameKo;
        _facilityNameEn = row.FacilityNameEn;

        _refundAmount = row.RefundAmount;
        _buildCost = row.BuildCost;
        _upgradeCost = row.UpgradeCost;
        _unlockRevenue = row.UnlockRevenue;
        _usageFee = row.UsageFee;

        _fatigueEffectPerTick = row.FatigueEffectPerTick;
        _thirstEffectPerTick = row.ThirstEffectPerTick;
        _hungerEffectPerTick = row.HungerEffectPerTick;
        _shopEffectPerTick = row.ShopEffectPerTick;
        _trainingEffectPerTick = row.TrainingEffectPerTick;

        Debug.Log($"[FacilityRuntimeData] 적용 완료 | FacilityID={_facilityID}, Fee={_usageFee}, Build={_buildCost}, Upgrade={_upgradeCost}");
        OnDataChanged?.Invoke();
    }
}