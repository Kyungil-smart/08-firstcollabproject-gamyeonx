using System;
using UnityEngine;

[Serializable]
public class GuestStates
{
    [Header("손님 상태")]
    [SerializeField] private int _visitorID;
    [SerializeField, Range(0, 100)] private int _hunger;
    [SerializeField, Range(0, 100)] private int _thirst;
    [SerializeField, Range(0, 100)] private int _fatigue;

    public int VisitorID => _visitorID;
    public int Hunger => _hunger;
    public int Thirst => _thirst;
    public int Fatigue => _fatigue;

    public event Action OnStatesChanged;

    public void Initialize(int visitorID, int hunger, int thirst, int fatigue)
    {
        _visitorID = visitorID;
        _hunger = ClampValue(hunger);
        _thirst = ClampValue(thirst);
        _fatigue = ClampValue(fatigue);

        Debug.Log($"[GuestStates] 초기화 완료 | {GetDebugText()}");
        RaiseStatesChanged();
    }

    public int GetNeedValue(EGuestNeedType needType)
    {
        switch (needType)
        {
            case EGuestNeedType.Hunger:
                return _hunger;
            case EGuestNeedType.Thirst:
                return _thirst;
            case EGuestNeedType.Fatigue:
                return _fatigue;
            default:
                return 0;
        }
    }

    public void SetNeedValue(EGuestNeedType needType, int value)
    {
        int clampedValue = ClampValue(value);

        switch (needType)
        {
            case EGuestNeedType.Hunger:
                _hunger = clampedValue;
                break;
            case EGuestNeedType.Thirst:
                _thirst = clampedValue;
                break;
            case EGuestNeedType.Fatigue:
                _fatigue = clampedValue;
                break;
            default:
                Debug.Log($"[GuestStates] SetNeedValue 실패. 잘못된 needType={needType}");
                return;
        }

        RaiseStatesChanged();
    }

    public void IncreaseAllNeedsByWanderTick()
    {
        _hunger = ClampValue(_hunger + 1);
        _thirst = ClampValue(_thirst + 1);
        _fatigue = ClampValue(_fatigue + 1);

        RaiseStatesChanged();
    }

    public bool HasAnyNeedReachedMax()
    {
        return _hunger >= 100 || _thirst >= 100 || _fatigue >= 100;
    }

    public void ApplyFacilityEffect(FacilityEffectRow effectRow)
    {
        if (effectRow == null)
        {
            return;
        }

        _hunger = ClampValue(_hunger + effectRow.HungerEffectPerTick);
        _thirst = ClampValue(_thirst + effectRow.ThirstEffectPerTick);
        _fatigue = ClampValue(_fatigue + effectRow.FatigueEffectPerTick);

        RaiseStatesChanged();
    }

    public string GetDebugText()
    {
        return $"VisitorID={_visitorID}, Hunger={_hunger}, Thirst={_thirst}, Fatigue={_fatigue}";
    }

    private int ClampValue(int value)
    {
        return Mathf.Clamp(value, 0, 100);
    }

    private void RaiseStatesChanged()
    {
        OnStatesChanged?.Invoke();
    }
}