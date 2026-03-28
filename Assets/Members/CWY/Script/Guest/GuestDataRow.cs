using System;
using UnityEngine;

[Serializable]
public class GuestDataRow
{
    [Header("¼Õ´Ô ±âº»°ª")]
    [SerializeField] private int _visitorID;
    [SerializeField, Range(0, 100)] private int _hunger = 0;
    [SerializeField, Range(0, 100)] private int _thirst = 0;
    [SerializeField, Range(0, 100)] private int _fatigue = 0;

    public int VisitorID => _visitorID;
    public int Hunger => _hunger;
    public int Thirst => _thirst;
    public int Fatigue => _fatigue;

    public void SetData(string[] cols)
    {
        _visitorID = int.Parse(cols[0]);
        _hunger = ClampValue(int.Parse(cols[1]));
        _thirst = ClampValue(int.Parse(cols[2]));
        _fatigue = ClampValue(int.Parse(cols[3]));
    }

    public string GetDebugText()
    {
        return $"VisitorID={_visitorID}, Hunger={_hunger}, Thirst={_thirst}, Fatigue={_fatigue}";
    }

    private int ClampValue(int value)
    {
        return Mathf.Clamp(value, 0, 100);
    }
}