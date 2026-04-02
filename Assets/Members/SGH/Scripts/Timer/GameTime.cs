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

            // 중요:
            // 기존처럼 180초에 자동으로 다음 주차로 넘기지 않는다.
            // 턴 종료 및 다음 주차 이동은 TurnGuestExitManager가 담당한다.
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

    // 테스트용:
    // 손님이 모두 퇴장했거나 4분 강제 종료가 끝나면
    // TurnGuestExitManager가 이 함수를 호출해서 다음 주차로 넘긴다.
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
}

/*
[Unity 연결 방법]
1. 기존 GameTime.cs를 이 코드로 교체한다.
2. 180초 자동 주차 증가 코드는 제거했다.
3. 이제 다음 주차 이동은 TurnGuestExitManager가 HandleTurnFinishedForTest()를 호출해서 처리한다.
4. 테스트가 끝난 뒤 준비시간 UI가 생기면 이 함수를 준비시간 진입 함수로 바꾸면 된다.
*/