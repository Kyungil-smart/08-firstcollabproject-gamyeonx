using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnEndUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _root;
    [SerializeField] private TextMeshProUGUI _turnVisitorText; //이 턴에 방문한 손님 수
    [SerializeField] private TextMeshProUGUI _turnInComeText; // 이 턴에 얻은 수입
    [SerializeField] private TextMeshProUGUI _TotalcomeText; // 총 수입
    [SerializeField] private Button _nextWeekButton;

    [Header("참조 스크립트")]
    [SerializeField] private GameTime _gameTime;

    private int _todayVisitorCount; // 이번 턴에 방문한 손님 수
    private int _turnIncome; // 이번 턴에 얻은 수입

    private void Awake()
    {
        if (_root != null)
        {
            _root.SetActive(false);
        }
        if (_nextWeekButton != null)
        {
            _nextWeekButton.onClick.AddListener(HandleClickNextWeekButton);
        }
    }

    public void ResetTurnData()
    {
        _todayVisitorCount = 0;
        _turnIncome = 0;

        Debug.Log("턴 데이터 초기화");
    }

    public void AddVisitor()
    {
        _todayVisitorCount++;
        Debug.Log("방문자 수 증가 | Today Visitor Count=" + _todayVisitorCount);
    }
    public void AddIncome(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _turnIncome += amount;
    }

    public void Show()
    {
        int totalIncome = GoldTest.Instance != null ? GoldTest.Instance.IncreasedGold : 0;
        _turnVisitorText.text = $"이번 턴 방문자 수: {_todayVisitorCount}";
        _turnInComeText.text = $"이번 턴 수입: {_turnIncome}원";
        _TotalcomeText.text = $"총 수입: {totalIncome}원";

        Time.timeScale = 0f;
        _root.SetActive(true);

        Debug.Log("턴 종료 UI 표시");
    }

    private void HandleClickNextWeekButton()
    {
        Time.timeScale = 1f;
        if (_root != null)
        {
            _root.SetActive(false);
        }

        if (_gameTime != null)
        {
            _gameTime.AdvanceToNextWeek();
        }

        ResetTurnData();
    }

}
