using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class EventUI : MonoBehaviour
{
    [Header("UI 구성 요소")]
    [SerializeField] private Button _nextButton; // 화면 전체를 덮는 투명 버튼 혹은 '다음' 버튼
    [SerializeField] private GameObject _enterButtonObj; // 마지막에 나타날 '확인' 버튼
    
    [Header("대화 내용")]
    [TextArea(3, 5)]
    public List<TextMeshProUGUI> Texts = new List<TextMeshProUGUI>();
    
    [Header("튜토리얼 연출 오브젝트")]
    [SerializeField] private GameObject _buildGuideUI;    // 건설 안내
    [SerializeField] private GameObject _upgradeGuideUI;  // 업그레이드 안내
    [SerializeField] private GameObject _BackGround;  // 안내시 백그라운드 삭제용
    private int _currentIndex = 0;

    public void StartConversation()
    {
        _currentIndex = 0;
        _enterButtonObj.SetActive(false); // 시작할 땐 확인 버튼 숨김
        _nextButton.gameObject.SetActive(true);
        
        UpdateUI();
    }

    public void OnClickNext()
    {
        _currentIndex++;

        if (_currentIndex < Texts.Count)
        {
            UpdateUI();
        }
        
        // 마지막 문장에 도달하면
        if (_currentIndex == Texts.Count - 1)
        {
            _nextButton.gameObject.SetActive(false); // 다음 클릭 버튼 비활성화
            _enterButtonObj.SetActive(true);        // 확인 버튼 활성화
        }
    }
    
    public void TutorialOnClickNext()
    {
        _currentIndex++;

        if (_currentIndex < Texts.Count)
        {
            UpdateUI();
        }

        if (_currentIndex == 12)
        {
            _BackGround.SetActive(false);
            // 건물 생성 튜토리얼 관련 버튼 이미지 및 버튼 활성화
            // 버튼 클릭 시 다시 다른애들 활성화
        }
        
        if (_currentIndex == 15)
        {
            _BackGround.SetActive(false);
            // 건물 업그레이드 튜토리얼 관련 버튼 이미지 및 버튼 활성화
            // 버튼 클릭 시 다시 다른애들 활성화
        }
        
        // 마지막 문장에 도달하면
        if (_currentIndex == Texts.Count - 1)
        {
            _nextButton.gameObject.SetActive(false); // 다음 클릭 버튼 비활성화
            _enterButtonObj.SetActive(true);        // 확인 버튼 활성화
        }
    }

    private void UpdateUI()
    {
        foreach(var t in Texts) t.gameObject.SetActive(false);
        if (_currentIndex < Texts.Count)
        {
            Texts[_currentIndex].gameObject.SetActive(true);
        }
    }
    
    public void ResumeTutorial()
    {
        if (EventManager.Instance.IsTutorial)
        {
            _buildGuideUI.SetActive(false);
            _upgradeGuideUI.SetActive(false);
            _BackGround.SetActive(true);
            _nextButton.gameObject.SetActive(true); // 다시 화면 클릭 가능하게
            TutorialOnClickNext(); // 다음 대사로 바로 넘겨줌
        }
    }
}
