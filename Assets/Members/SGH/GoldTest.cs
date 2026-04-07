using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldTest : MonoBehaviour
{
    public static GoldTest Instance;
    public int _testGold;
    public int IncreasedGold = 0;
    [SerializeField] private TextMeshProUGUI _goldText;
    //========= 밑 삭제
    [SerializeField] private TextMeshProUGUI _increasedGoldText;

    public int TestGoldValue
    {
        get => _testGold;
        set
        {
            _testGold = value;
            UpdateUI();
            //EventManager.Instance.CheckGoldEvents(IncreasedGold);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        UpdateUI();
        UpdateIncreasedGoldUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PayMoney(10000);
            Debug.Log($"현재 누적 수익 {IncreasedGold}");
        }
    }

    public void PayMoney(int value)
    {
        TestGoldValue += value;
        IncreasedGold += value;
        //===========밑 삭제
        UpdateIncreasedGoldUI();

        EventManager.Instance.CheckGoldEvents(IncreasedGold);
    }

    public void GetFacilityRefundAmount(int value)
    {
        TestGoldValue += value;
    }

    public void PlayerUseMoney(int value)
    {
        if (TestGoldValue < value)
        {
            Debug.Log("골드 부족");
            return;
        }
        
        TestGoldValue -= value;
    }

    private void UpdateUI()
    {
        _goldText.text = $"{_testGold}G";
    }

    //===========밑 삭제
    private void UpdateIncreasedGoldUI()
    {
        _increasedGoldText.text = $"{ IncreasedGold }누적골드";
    }
}