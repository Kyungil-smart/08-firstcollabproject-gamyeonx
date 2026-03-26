using System;
using UnityEngine;

[Serializable]
public class FacilityEffectRow
{
    [Header("시설 정보")]
    [SerializeField] private int _facilityID;
    [SerializeField] private EFacilityType _facilityType = EFacilityType.None;

    [Header("일반 손님 선택 가능 여부")]
    [SerializeField] private bool _isSelectableByNormalGuest = false;

    [Header("이용 중 틱 효과")]
    [SerializeField] private int _hungerEffectPerTick;
    [SerializeField] private int _thirstEffectPerTick;
    [SerializeField] private int _fatigueEffectPerTick;
    [SerializeField] private int _satisfactionEffectPerTick;

    public int FacilityID => _facilityID;
    public EFacilityType FacilityType => _facilityType;
    public bool IsSelectableByNormalGuest => _isSelectableByNormalGuest;

    public int HungerEffectPerTick => _hungerEffectPerTick;
    public int ThirstEffectPerTick => _thirstEffectPerTick;
    public int FatigueEffectPerTick => _fatigueEffectPerTick;
    public int SatisfactionEffectPerTick => _satisfactionEffectPerTick;

    public void SetData(string[] cols)
    {
        _facilityID = int.Parse(cols[0]);
        _facilityType = ParseFacilityType(cols[1]);
        _isSelectableByNormalGuest = ParseBool(cols[2]);
        _hungerEffectPerTick = int.Parse(cols[3]);
        _thirstEffectPerTick = int.Parse(cols[4]);
        _fatigueEffectPerTick = int.Parse(cols[5]);
        _satisfactionEffectPerTick = int.Parse(cols[6]);
    }

    public string GetDebugText()
    {
        return $"FacilityID={_facilityID}, FacilityType={_facilityType}, Selectable={_isSelectableByNormalGuest}, HungerTick={_hungerEffectPerTick}, ThirstTick={_thirstEffectPerTick}, FatigueTick={_fatigueEffectPerTick}, SatisfactionTick={_satisfactionEffectPerTick}";
    }

    private EFacilityType ParseFacilityType(string value)
    {
        if (Enum.TryParse(value, true, out EFacilityType result))
        {
            return result;
        }

        Debug.LogWarning($"[FacilityEffectRow] 잘못된 FacilityType 입니다. value={value}");
        return EFacilityType.None;
    }

    private bool ParseBool(string value)
    {
        string normalized = value.Trim().ToLower();
        return normalized == "true" || normalized == "1" || normalized == "y" || normalized == "yes";
    }
}