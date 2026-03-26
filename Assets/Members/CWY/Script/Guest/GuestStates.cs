using System;
using UnityEngine;

public class GuestStates
{
    [Header("Guest States")]
    //방문객 고유 ID
    [SerializeField] private int _visitorID;

    //Guest 배고픔 수치(높을수록 더 배고픔)
    [SerializeField, Range(0, 100)] private int _hunger = 0;

    //Guest 목마름 수치(높을수록 더 목마름)
    [SerializeField, Range(0, 100)] private int _thirst = 0;

    //Guest 피로도 수치(높을수록 더 피로함)
    [SerializeField, Range(0, 100)] private int _fatigue = 0;

    //Guest 청결도 수치(높을수록 더 깨끗함)
    [SerializeField, Range(0, 100)] private int _cleanliness = 0;

    //Guest 배고픔 수치(높을수록 더 배고픔)
    [SerializeField, Range(0, 100)] private int _satisfaction = 0;

    public int visitorID => _visitorID;
    public int hunger => _hunger;
    public int thirst => _thirst;
    public int fatigue => _fatigue;
    public int cleanliness => _cleanliness;
    public int Satisfaction => _satisfaction;

    public event Action OnStatesChanged;

    public void Initialize(
        int visitorID,
        int hunger,
        int thirst,
        int fatigue,
        int cleanliness,
        int satisfaction)
    {
        _visitorID = visitorID;
        _hunger = ClampNeed(hunger);
        _thirst = ClampNeed(thirst);
        _fatigue = ClampNeed(fatigue);
        _cleanliness = ClampNeed(cleanliness);
        _satisfaction = ClampNeed(satisfaction);

        Debug.Log($"[GuestNeeds] Initialized | {GetDebugText()}");
        RaiseStatesChanged();
    }

    public int GetHungerScore()
    {
        return _hunger;
    }
    public int GetThirstScore()
    { 
        return _thirst; 
    }
    public int GetFatigueScore()
    {
        return _fatigue;
    }
    public int GetCleanlinessScore()
    {
        return 100 - _cleanliness;
    }

    public void AddHunger(int value)
    {
        _hunger = ClampNeed(_hunger + value);
        RaiseStatesChanged();
    }

    public void AddThirst(int value)
    {
        _thirst = ClampNeed(_thirst + value);
        RaiseStatesChanged();
    }

    public void AddFatigue(int value)
    {
        _fatigue = ClampNeed(_fatigue + value);
        RaiseStatesChanged();
    }

    public void AddCleanliness(int value)
    {
        _cleanliness = ClampNeed(_cleanliness + value);
        RaiseStatesChanged();
    }

    public void AddSatisfaction(int value)
    {
        _satisfaction = ClampNeed(_satisfaction + value);
        RaiseStatesChanged();
    }


    private int ClampNeed(int value)
    {
        return Mathf.Clamp(value, 0, 100);
    }
    public string GetDebugText()
    {
        return $"VisitorID={_visitorID}, Hunger={_hunger}, Thirst={_thirst}, Fatigue={_fatigue}, Cleanliness={_cleanliness}, Satisfaction={_satisfaction}";
    }
    private void RaiseStatesChanged()
    {
        OnStatesChanged?.Invoke();
    }

    public void ApplyFacilityEffect(FacilityEffectRow effectRow)
    {
        if (effectRow == null)
        {
            Debug.LogWarning("[GuestStates] ApplyFacilityEffect failed. EffectRow is null.");
            return;
        }

        AddHunger(effectRow.HungerEffect);
        AddThirst(effectRow.ThirstEffect);
        AddFatigue(effectRow.FatigueEffect);
        AddCleanliness(effectRow.CleanEffect);
        AddSatisfaction(effectRow.SatisfactionEffect);

        Debug.Log($"[GuestStates] Applied Facility Effect | {effectRow.GetDebugText()}");
    }
}
