using UnityEngine;
using TMPro;

public class GameTime : MonoBehaviour
{
    private float _userTime;
    private float _userTimeUnit = 180f;
    private int _userWeek;
    private int _userWeekUnit = 4;
    private int _userMonth;
    private int _userMonthUnit = 12;
    private int _userYear;

    private void Update()
    {
        _userTime += Time.deltaTime;
        if (_userTime >= _userTimeUnit)
        {
            _userTime -= _userTimeUnit;
            _userWeek++;
        }
        if (_userWeek >= _userWeekUnit)
        {
            _userWeek = 0;
            _userMonth++;
        }
        if (_userMonth >= _userMonthUnit)
        {
            _userMonth = 0;
            _userYear++;
        }
    }
}
