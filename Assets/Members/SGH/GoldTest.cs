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

    public int TestGoldValue
    {
        get => _testGold;
        set
        {
            _testGold = value;
            UpdateUI();
            EventManager.Instance.CheckGoldEvents(IncreasedGold);
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PayMoney(1000);
            Debug.Log($"현재 누적 수익 {IncreasedGold}");
        }
    }

    public void PayMoney(int value)
    {
        TestGoldValue += value;
        IncreasedGold += value;
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
}