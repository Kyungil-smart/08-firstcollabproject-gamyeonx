using System;
using System.Collections.Generic;
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

    public float _userTime;
    private float _userTimeUnit = 180f;
    private float _nightTime = 120f;
    public int _userWeek;
    

    private void Awake()
    {
        _nightImageObject.SetActive(false);
        if (SaveManager.Instance.LoadMap)
        {
            _userWeek = SaveManager.Instance.data.UserWeek;
            Debug.Log(_userWeek);
        }
        else _userWeek = 0;
    }
    
    public float UserTime
    {
        get => _userTime;
        set
        {
            _userTime = value;
            
            UpdateTimeUI();
            
            if (_userTime >= _nightTime && _userTime < _userTimeUnit)
            {
                if (!_nightImageObject.activeSelf) 
                {
                    _nightImageObject.SetActive(true);
                }
            }
            
            if (_userTime >= _userTimeUnit)
            {
                _userTime -= _userTimeUnit;
                _nightImageObject.SetActive(false);
                UserWeek++;
                SaveManager.Instance.Save();
            }
        }
    }

    public int UserWeek
    {
        get => _userWeek;
        set
        {
            _userWeek = value;
            UpdateWeekUI();
            EventManager.Instance.CheckWeekEvents(_userWeek);
        }
    }

    private void Start()
    {
        _week.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Week", new object[] { _userWeek });
        /*_month.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Month", new object[] { _userMonth });
        _year.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Year", new object[] { _userYear });*/
        _time.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Second", new object[] { (int)_userTime });
    }

    /*private void Update()
    {
        _userTime += Time.deltaTime;
        _time.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Second", new object[] { (int)_userTime });
        _week.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Week", new object[] { _userWeek });

        if (_userTime >= _nightTime && _userTime < _userTimeUnit && !_nightImageObject.activeSelf)
        {
            _nightImageObject.SetActive(true);
        }
        if (_userTime >= _userTimeUnit)
        {
            _userTime -= _userTimeUnit;
            _nightImageObject.SetActive(false);
            _userWeek++;
            SaveManager.Instance.Save();
        }
        /*if (_userWeek >= _userWeekUnit)
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
        }#1#
    }*/
    
    private void Update()
    {
        UserTime += Time.deltaTime;
    }

    private void UpdateTimeUI()
    {
        _time.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Second", new object[] { (int)_userTime });
    }

    private void UpdateWeekUI()
    {
        _week.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Week", new object[] { _userWeek });
    }
}
