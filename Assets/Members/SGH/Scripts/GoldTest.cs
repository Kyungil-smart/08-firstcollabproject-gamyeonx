using UnityEngine;
using TMPro;

public class GoldTest : MonoBehaviour
{
    [SerializeField] private int _testGold;
    [SerializeField] private TextMeshProUGUI _goldText;

    public int TestGoldValue
    {
        get => _testGold;
        set
        {
            _testGold = value;
            UpdateUI();
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
}