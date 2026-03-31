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

    [Header("특수시설 사용 가능 여부")]
    [SerializeField] private bool _canUseShop;
    [SerializeField] private bool _canUseTraining;

    [Header("특수시설 상태")]
    [SerializeField, Range(0, 100)] private int _shopNeed;
    [SerializeField, Range(0, 100)] private int _trainingNeed;

    public int VisitorID => _visitorID;
    public int Hunger => _hunger;
    public int Thirst => _thirst;
    public int Fatigue => _fatigue;
    public bool CanUseShop => _canUseShop;
    public bool CanUseTraining => _canUseTraining;
    public int ShopNeed => _shopNeed;
    public int TrainingNeed => _trainingNeed;

    public event Action OnStatesChanged;

    public void Initialize(
        int visitorID,
        int hunger,
        int thirst,
        int fatigue,
        bool canUseShop,
        int shopNeed,
        bool canUseTraining,
        int trainingNeed)
    {
        _visitorID = visitorID;
        _hunger = ClampValue(hunger);
        _thirst = ClampValue(thirst);
        _fatigue = ClampValue(fatigue);

        _canUseShop = canUseShop;
        _shopNeed = canUseShop ? ClampValue(shopNeed) : 0;

        _canUseTraining = canUseTraining;
        _trainingNeed = canUseTraining ? ClampValue(trainingNeed) : 0;

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
            case EGuestNeedType.Shop:
                return _canUseShop ? _shopNeed : 0;
            case EGuestNeedType.Training:
                return _canUseTraining ? _trainingNeed : 0;
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

            case EGuestNeedType.Shop:
                if (_canUseShop)
                {
                    _shopNeed = clampedValue;
                }
                break;

            case EGuestNeedType.Training:
                if (_canUseTraining)
                {
                    _trainingNeed = clampedValue;
                }
                break;

            default:
                Debug.LogWarning($"[GuestStates] SetNeedValue 실패 | 잘못된 needType={needType}");
                return;
        }

        RaiseStatesChanged();
    }

    public void IncreaseAllNeedsByWanderTick()
    {
        _hunger = ClampValue(_hunger + 1);
        _thirst = ClampValue(_thirst + 1);
        _fatigue = ClampValue(_fatigue + 1);

        if (_canUseShop)
        {
            _shopNeed = ClampValue(_shopNeed + 1);
        }

        if (_canUseTraining)
        {
            _trainingNeed = ClampValue(_trainingNeed + 1);
        }

        RaiseStatesChanged();
    }

    public bool HasAnyNeedReachedMax()
    {
        if (_hunger >= 100 || _thirst >= 100 || _fatigue >= 100)
        {
            return true;
        }

        if (_canUseShop && _shopNeed >= 100)
        {
            return true;
        }

        if (_canUseTraining && _trainingNeed >= 100)
        {
            return true;
        }

        return false;
    }

    public void ApplyFacilityEffect(FacilityEffectRow effectRow)
    {
        if (effectRow == null)
        {
            Debug.LogWarning("[GuestStates] ApplyFacilityEffect 실패 | effectRow가 null입니다.");
            return;
        }

        _hunger = ClampValue(_hunger + effectRow.HungerEffectPerTick);
        _thirst = ClampValue(_thirst + effectRow.ThirstEffectPerTick);
        _fatigue = ClampValue(_fatigue + effectRow.FatigueEffectPerTick);

        if (_canUseShop)
        {
            _shopNeed = ClampValue(_shopNeed + effectRow.ShopEffectPerTick);
        }

        if (_canUseTraining)
        {
            _trainingNeed = ClampValue(_trainingNeed + effectRow.TrainingEffectPerTick);
        }

        Debug.Log($"[GuestStates] 시설 효과 적용 완료 | {GetDebugText()}, ShopNeed={_shopNeed}, TrainingNeed={_trainingNeed}");
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