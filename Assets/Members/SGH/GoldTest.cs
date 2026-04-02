using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldTest : MonoBehaviour
{
    public static GoldTest Instance;
    public int _testGold;
    [SerializeField] private TextMeshProUGUI _goldText;

    public int TestGoldValue
    {
        get => _testGold;
        set
        {
            _testGold = value;
            UpdateUI();
            
            if (_testGold >= 10)
            {
                string eventKey = "WEEK_10_EVENT";
                EventManager.Instance.CheckGoldEvents(_testGold);
            }
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

    public void PayMoney(int value)
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
}