using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldTest : MonoBehaviour
{
    public int _testGold;
    [SerializeField] private TextMeshProUGUI _goldText;
    
    private HashSet<string> _triggeredEvents = new HashSet<string>();

    public int TestGoldValue
    {
        get => _testGold;
        set
        {
            _testGold = value;
            UpdateUI();
            
            if (_testGold >= 10 && !_triggeredEvents.Contains("WEEK_10_EVENT"))
            {
                dasfasdfEvent();
                _triggeredEvents.Add("WEEK_10_EVENT");
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
    }

    private void UpdateUI()
    {
        _goldText.text = $"{_testGold}G";
    }
    
    private void dasfasdfEvent() { /*이벤트 로직*/ }
}