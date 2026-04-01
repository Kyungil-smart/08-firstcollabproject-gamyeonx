using System;
using UnityEngine;

[Serializable]
public class FacilityEffectRow
{
    [Header("시설 정보")]
    [SerializeField] private string _facilityID;
    [SerializeField] private EFacilityType _facilityType = EFacilityType.None;
    [SerializeField] private string _facilityNameKo;
    [SerializeField] private string _facilityNameEn;

    [Header("비용 정보")]
    [SerializeField] private int _refundAmount;
    [SerializeField] private int _buildCost;
    [SerializeField] private int _upgradeCost;
    [SerializeField] private int _unlockRevenue;
    [SerializeField] private int _usageFee;

    [Header("이용 중 틱 효과")]
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

    public void SetData(string[] cols)
    {
        _facilityID = GetSafeValue(cols, 1);
        _facilityType = ParseFacilityType(GetSafeValue(cols, 2));
        _facilityNameKo = GetSafeValue(cols, 3);
        _facilityNameEn = GetSafeValue(cols, 4);

        // 5번 컬럼(normal_guest_available) 사용 안 함
        _refundAmount = ParseInt(GetSafeValue(cols, 6));
        _buildCost = ParseInt(GetSafeValue(cols, 7));
        _upgradeCost = ParseInt(GetSafeValue(cols, 8));
        _unlockRevenue = ParseInt(GetSafeValue(cols, 9));

        // 10번 컬럼(capacity) 사용 안 함
        _usageFee = ParseInt(GetSafeValue(cols, 11));

        _fatigueEffectPerTick = ParseInt(GetSafeValue(cols, 12));
        _thirstEffectPerTick = ParseInt(GetSafeValue(cols, 13));
        _hungerEffectPerTick = ParseInt(GetSafeValue(cols, 14));
        _shopEffectPerTick = ParseInt(GetSafeValue(cols, 15));
        _trainingEffectPerTick = ParseInt(GetSafeValue(cols, 16));
    }

    private string GetSafeValue(string[] cols, int index)
    {
        if (cols == null || index < 0 || index >= cols.Length)
        {
            return string.Empty;
        }

        return cols[index].Trim();
    }

    private int ParseInt(string value)
    {
        if (int.TryParse(value.Trim(), out int result))
        {
            return result;
        }

        return 0;
    }

    private EFacilityType ParseFacilityType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return EFacilityType.None;
        }

        if (Enum.TryParse(value, true, out EFacilityType result))
        {
            return result;
        }

        Debug.LogWarning($"[FacilityEffectRow] 잘못된 FacilityType 입니다. value={value}");
        return EFacilityType.None;
    }
}

/*
유니티 적용 방법
1. 기존 FacilityEffectRow.cs를 이 코드로 교체합니다.
2. 현재 시트 구조 기준으로 5, 10 컬럼은 무시합니다.
3. 이제 비용/요금/효과 관련 값이 올바른 칼럼에서 읽힙니다.
*/