using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldTest : MonoBehaviour
{
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
            
            if (IncreasedGold >= 10000)
            {
                EventManager.Instance.CheckGoldEvents(IncreasedGold);
            }
        }
    }

    private void Awake()
    {
        UpdateUI();
    }

    public void PayMoney(int value)
    {
        TestGoldValue += value;
        IncreasedGold += value;
    }

    private void UpdateUI()
    {
        _goldText.text = $"{_testGold}G";
    }
}