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
        else
        {
            _userWeek = 0;
        }
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
        _time.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Second", new object[] { (int)_userTime });
    }

    private void Update()
    {
        UserTime += Time.deltaTime;
    }

    public void HandleTurnFinishedForTest()
    {
        _userTime = 0f;
        _nightImageObject.SetActive(false);
        UserWeek++;
        SaveManager.Instance.Save();

        Debug.Log($"[GameTime] 다음 주차로 이동 | CurrentWeek={_userWeek}");
    }

    private void UpdateTimeUI()
    {
        _time.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Second", new object[] { (int)_userTime });
    }

    private void UpdateWeekUI()
    {
        _week.text = LocalizationSettings.StringDatabase.GetLocalizedString("ProjectTable", "UI_Week", new object[] { _userWeek });
    }

    public void AdvanceToNextWeek()
    {
        _userTime = 0f;
        _nightImageObject.SetActive(false);
        UserWeek++;
        SaveManager.Instance.Save();
        Debug.Log($"[GameTime] 다음 주차로 이동 | CurrentWeek={_userWeek}");
    }
}