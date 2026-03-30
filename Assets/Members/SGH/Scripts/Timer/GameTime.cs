using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class GameTime : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _year;
    [SerializeField] private TextMeshProUGUI _month;
    [SerializeField] private TextMeshProUGUI _week;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private Image _nightImage;
    [SerializeField] private GameObject _nightImageObject;

    private float _userTime;
    private float _userTimeUnit = 180f;
    private float _nightTime = 120f;
    private int _userWeek;
    private int _userWeekUnit = 4;
    private int _userMonth;
    private int _userMonthUnit = 12;
    private int _userYear;

    private void Awake()
    {
        _nightImageObject.SetActive(false);
        _week.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Week", new object[] { _userWeek });
        _month.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Month", new object[] { _userMonth });
        _year.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Year", new object[] { _userYear });
        _time.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Second", new object[] { (int)_userTime });
    }

    private void Update()
    {
        _userTime += Time.deltaTime;
        _time.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Second", new object[] { (int)_userTime });

        if (_userTime >= _nightTime && _userTime < _userTimeUnit && !_nightImageObject.activeSelf)
        {
            _nightImageObject.SetActive(true);
        }
        if (_userTime >= _userTimeUnit)
        {
            _userTime -= _userTimeUnit;
            _nightImageObject.SetActive(false);
            _userWeek++;
            _week.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Week", new object[] { _userWeek });
        }
        if (_userWeek >= _userWeekUnit)
        {
            _userWeek = 0;
            _week.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Week", new object[] { _userWeek });
            _userMonth++;
            _month.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Month", new object[] { _userMonth });
        }
        if (_userMonth >= _userMonthUnit)
        {
            _userMonth = 0;
            _month.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Month", new object[] { _userMonth });
            _userYear++;
            _year.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Year", new object[] { _userYear });
        }
    }
}
