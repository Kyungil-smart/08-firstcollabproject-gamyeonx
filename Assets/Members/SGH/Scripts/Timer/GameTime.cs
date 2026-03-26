using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        _week.text = $"{_userWeek}주";
        _month.text = $"{_userMonth}월";
        _year.text = $"{_userYear}년";
        _time.text = $"{_userTime}초";
    }

    private void Update()
    {
        _userTime += Time.deltaTime;
        _time.text = $"{(int)_userTime}초";

        if (_userTime >= _nightTime && _userTime < _userTimeUnit && !_nightImageObject.activeSelf)
        {
            _nightImageObject.SetActive(true);
        }
        if (_userTime >= _userTimeUnit)
        {
            _userTime -= _userTimeUnit;
            _nightImageObject.SetActive(false);
            _userWeek++;
            _week.text = $"{_userWeek}주";
        }
        if (_userWeek >= _userWeekUnit)
        {
            _userWeek = 0;
            _week.text = $"{_userWeek}주";
            _userMonth++;
            _month.text = $"{_userMonth}월";
        }
        if (_userMonth >= _userMonthUnit)
        {
            _userMonth = 0;
            _month.text = $"{_userMonth}월";
            _userYear++;
            _year.text = $"{_userYear}년";
        }
    }
}
