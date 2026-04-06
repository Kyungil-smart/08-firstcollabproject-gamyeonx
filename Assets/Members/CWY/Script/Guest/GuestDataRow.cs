using System;
using UnityEngine;

[Serializable]
public class GuestDataRow
{
    [Header("손님 정보")]
    [SerializeField] private int _visitorID;
    [SerializeField] private bool _isAdventurer = false;
    [SerializeField] private string _adventurerGrade = "None";

    [Header("초기 상태값")]
    [SerializeField] private int _fatigue;
    [SerializeField] private int _thirst;
    [SerializeField] private int _hunger;

    [Header("특수 시설 사용 정보")]
    [SerializeField] private bool _useShop = false;
    [SerializeField] private int _shopNeed;
    [SerializeField] private bool _useTraining = false;
    [SerializeField] private int _trainingNeed;

    [Header("스폰 정보")]
    [SerializeField] private int _spawnWeight = 1;

    public int VisitorID => _visitorID;
    public bool IsAdventurer => _isAdventurer;
    public string AdventurerGrade => _adventurerGrade;

    public int Fatigue => _fatigue;
    public int Thirst => _thirst;
    public int Hunger => _hunger;

    public bool UseShop => _useShop;
    public int ShopNeed => _shopNeed;
    public bool UseTraining => _useTraining;
    public int TrainingNeed => _trainingNeed;

    public int SpawnWeight => _spawnWeight;

    public void SetData(string[] cols)
    {
        _visitorID = ParseVisitorID(cols[1]);
        _isAdventurer = ParseBool(cols[3]);
        _adventurerGrade = cols[4].Trim();

        _fatigue = ClampValue(ParseInt(cols[5]));
        _thirst = ClampValue(ParseInt(cols[6]));
        _hunger = ClampValue(ParseInt(cols[7]));

        _useShop = ParseBool(cols[8]);
        _shopNeed = _useShop ? ClampValue(ParseInt(cols[9])) : 0;

        _useTraining = ParseBool(cols[10]);
        _trainingNeed = _useTraining ? ClampValue(ParseInt(cols[11])) : 0;

        _spawnWeight = Mathf.Max(0, ParseInt(cols[12]));
    }

    public string GetDebugText()
    {
        return $"VisitorID={_visitorID}, IsAdventurer={_isAdventurer}, Grade={_adventurerGrade}, " +
               $"Fatigue={_fatigue}, Thirst={_thirst}, Hunger={_hunger}, " +
               $"UseShop={_useShop}, ShopNeed={_shopNeed}, " +
               $"UseTraining={_useTraining}, TrainingNeed={_trainingNeed}, " +
               $"SpawnWeight={_spawnWeight}";
    }

    private int ParseVisitorID(string value)
    {
        string trimmed = value.Trim();

        if (int.TryParse(trimmed, out int id))
        {
            return id;
        }

        string numberOnly = "";

        for (int i = 0; i < trimmed.Length; i++)
        {
            if (char.IsDigit(trimmed[i]))
            {
                numberOnly += trimmed[i];
            }
        }

        if (int.TryParse(numberOnly, out int parsedID))
        {
            return parsedID;
        }

        Debug.LogWarning($"[GuestDataRow] VisitorID 실패 | value={value}");
        return 0;
    }

    private int ParseInt(string value)
    {
        if (int.TryParse(value.Trim(), out int result))
        {
            return result;
        }

        Debug.LogWarning($"[GuestDataRow] int 실패 | value={value}");
        return 0;
    }

    private bool ParseBool(string value)
    {
        string normalized = value.Trim().ToLower();
        return normalized == "true" || normalized == "1" || normalized == "y" || normalized == "yes";
    }

    private int ClampValue(int value)
    {
        return Mathf.Clamp(value, 0, 100);
    }
}