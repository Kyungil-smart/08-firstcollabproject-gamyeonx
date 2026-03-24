using System;
using UnityEngine;

[Serializable]
public class FacilityEffectRow
{
    [Header("Facility Info")]
    // 시설 고유 ID
    [SerializeField] private int _facilityID;

    // 시설 종류
    [SerializeField] private EFacilityType _eFacilityType = EFacilityType.None;

    [Header("Guest Effects")]
    // 시설 이용 시 배고픔 변화량
    [SerializeField] private int _hungerEffect;

    // 시설 이용 시 목마름 변화량
    [SerializeField] private int _thirstEffect;

    // 시설 이용 시 피로도 변화량
    [SerializeField] private int _fatigueEffect;

    // 시설 이용 시 청결도 변화량
    [SerializeField] private int _cleanEffect;

    // 시설 이용 시 만족도 변화량
    [SerializeField] private int _satisfactionEffect;

    public int FacilityID => _facilityID;
    public EFacilityType EFacilityType => _eFacilityType;

    public int HungerEffect => _hungerEffect;
    public int ThirstEffect => _thirstEffect;
    public int FatigueEffect => _fatigueEffect;
    public int CleanEffect => _cleanEffect;
    public int SatisfactionEffect => _satisfactionEffect;

    /// <summary>
    /// 시트 한 줄 데이터를 받아서 값 세팅
    /// 예상 컬럼 순서:
    /// 0 FacilityID
    /// 1 EFacilityType
    /// 2 HungerEffect
    /// 3 ThirstEffect
    /// 4 FatigueEffect
    /// 5 CleanEffect
    /// 6 SatisfactionEffect
    /// </summary>
    public void SetData(string[] cols)
    {
        _facilityID = int.Parse(cols[0]);
        _eFacilityType = ParseFacilityType(cols[1]);
        _hungerEffect = int.Parse(cols[2]);
        _thirstEffect = int.Parse(cols[3]);
        _fatigueEffect = int.Parse(cols[4]);
        _cleanEffect = int.Parse(cols[5]);
        _satisfactionEffect = int.Parse(cols[6]);
    }

    public string GetDebugText()
    {
        return $"FacilityID={_facilityID}, EFacilityType={_eFacilityType}, HungerEffect={_hungerEffect}, ThirstEffect={_thirstEffect}, FatigueEffect={_fatigueEffect}, CleanEffect={_cleanEffect}, SatisfactionEffect={_satisfactionEffect}";
    }

    private EFacilityType ParseFacilityType(string value)
    {
        if (Enum.TryParse(value, true, out EFacilityType result))
        {
            return result;
        }

        Debug.LogWarning($"[FacilityEffectRow] Invalid EFacilityType: {value}");
        return EFacilityType.None;
    }
}