using System;
using UnityEngine;


[Serializable]
public class GuestDataRow
{
    [Header("Guest Identity")]
    // 방문객 고유 ID
    [SerializeField] private int _visitorID;

    [Header("Initial States")]
    // 시작 배고픔
    [SerializeField, Range(0, 100)] private int _hunger = 0;

    // 시작 목마름
    [SerializeField, Range(0, 100)] private int _thirst = 0;

    // 시작 피로도
    [SerializeField, Range(0, 100)] private int _fatigue = 0;

    // 시작 청결도
    [SerializeField, Range(0, 100)] private int _cleanliness = 100;

    // 시작 만족도
    [SerializeField, Range(0, 100)] private int _satisfaction = 50;

    [Header("State Change Per Second")]
    // 초당 배고픔 변화량
    [SerializeField] private int _hungerDeltaPerSecond = 1;

    // 초당 목마름 변화량
    [SerializeField] private int _thirstDeltaPerSecond = 1;

    // 초당 피로도 변화량
    [SerializeField] private int _fatigueDeltaPerSecond = 1;

    // 초당 청결도 변화량
    [SerializeField] private int _cleanlinessDeltaPerSecond = -1;

    // 초당 만족도 변화량
    [SerializeField] private int _satisfactionDeltaPerSecond = 0;

    [Header("Behavior Settings")]
    // 의사결정 주기
    [SerializeField] private float _decisionInterval = 2f;

    // 배회 시간
    [SerializeField] private float _wanderDuration = 3f;

    public int VisitorID => _visitorID;
    public int Hunger => _hunger;
    public int Thirst => _thirst;
    public int Fatigue => _fatigue;
    public int Cleanliness => _cleanliness;
    public int Satisfaction => _satisfaction;

    public int HungerDeltaPerSecond => _hungerDeltaPerSecond;
    public int ThirstDeltaPerSecond => _thirstDeltaPerSecond;
    public int FatigueDeltaPerSecond => _fatigueDeltaPerSecond;
    public int CleanlinessDeltaPerSecond => _cleanlinessDeltaPerSecond;
    public int SatisfactionDeltaPerSecond => _satisfactionDeltaPerSecond;

    public float DecisionInterval => _decisionInterval;
    public float WanderDuration => _wanderDuration;

    /// <summary>
    /// Set row data from sheet columns.
    /// Expected column order:
    /// 0 VisitorID
    /// 1 Hunger
    /// 2 Thirst
    /// 3 Fatigue
    /// 4 Cleanliness
    /// 5 Satisfaction
    /// 6 HungerDeltaPerSecond
    /// 7 ThirstDeltaPerSecond
    /// 8 FatigueDeltaPerSecond
    /// 9 CleanlinessDeltaPerSecond
    /// 10 SatisfactionDeltaPerSecond
    /// 11 DecisionInterval
    /// 12 WanderDuration
    /// </summary>
    public void SetData(string[] cols)
    {
        _visitorID = int.Parse(cols[0]);
        _hunger = ClampNeed(int.Parse(cols[1]));
        _thirst = ClampNeed(int.Parse(cols[2]));
        _fatigue = ClampNeed(int.Parse(cols[3]));
        _cleanliness = ClampNeed(int.Parse(cols[4]));
        _satisfaction = ClampNeed(int.Parse(cols[5]));

        _hungerDeltaPerSecond = int.Parse(cols[6]);
        _thirstDeltaPerSecond = int.Parse(cols[7]);
        _fatigueDeltaPerSecond = int.Parse(cols[8]);
        _cleanlinessDeltaPerSecond = int.Parse(cols[9]);
        _satisfactionDeltaPerSecond = int.Parse(cols[10]);

        _decisionInterval = Mathf.Max(0.1f, float.Parse(cols[11]));
        _wanderDuration = Mathf.Max(0.1f, float.Parse(cols[12]));
    }

    public string GetDebugText()
    {
        return $"VisitorID={_visitorID}, Hunger={_hunger}, Thirst={_thirst}, Fatigue={_fatigue}, Cleanliness={_cleanliness}, Satisfaction={_satisfaction}, " +
               $"HungerDelta={_hungerDeltaPerSecond}, ThirstDelta={_thirstDeltaPerSecond}, FatigueDelta={_fatigueDeltaPerSecond}, CleanlinessDelta={_cleanlinessDeltaPerSecond}, SatisfactionDelta={_satisfactionDeltaPerSecond}, " +
               $"DecisionInterval={_decisionInterval}, WanderDuration={_wanderDuration}";
    }

    private int ClampNeed(int value)
    {
        return Mathf.Clamp(value, 0, 100);
    }
}