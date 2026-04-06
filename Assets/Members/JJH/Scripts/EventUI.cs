using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
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
    [SerializeField] private List<GameObject> _buildGuideUI;    // 건설 안내
    [SerializeField] private List<GameObject> _roadGuideUI;  // 길 안내
    [SerializeField] private GameObject _BackGround;  // 안내시 백그라운드 삭제용
    [SerializeField] private GameObject _namePlateGroup; // 이름표 배경
    [SerializeField] private TextMeshProUGUI _nameText; // 화자 이름표
    private int _currentIndex = 0;
    private int _currentGuideIndex = 0;

    public void StartConversation()
    {
        _currentIndex = 0;
        _enterButtonObj.SetActive(false); // 시작할 땐 확인 버튼 숨김
        _nextButton.gameObject.SetActive(true);
        CameraController cam = FindFirstObjectByType<CameraController>();
        cam.IsCrapting = true;
        
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

        if (_currentIndex == 11)
        {
            _buildGuideUI[0].gameObject.SetActive(true);
            _BackGround.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            Time.timeScale = 1f;
            CameraController cam = FindFirstObjectByType<CameraController>();
            cam.TutorialCameraMove();
            // 건물 생성 튜토리얼 관련 버튼 이미지 및 버튼 활성화
            // 버튼 클릭 시 다시 다른애들 활성화
        }
        
        if (_currentIndex == 13)
        {
            _roadGuideUI[0].gameObject.SetActive(true);
            _BackGround.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            Time.timeScale = 1f;
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
        
        string nameKey = $"TutorialText{_currentIndex}_Name";
        
        var table = LocalizationSettings.StringDatabase.GetTable("Tutorial");
        var entry = table?.GetEntry(nameKey);
        
        if (entry == null || string.IsNullOrWhiteSpace(entry.GetLocalizedString()))
        {
            // 이름 데이터가 아예 없거나 비어있으면 이름표 숨기기
            _namePlateGroup.SetActive(false);
        }
        else
        {
            // 이름이 있으면 텍스트 내용을 갈아끼우고 보여주기
            _nameText.text = entry.GetLocalizedString(); 
            _namePlateGroup.SetActive(true);
        }
    }
    
    public void ResumeTutorial()
    {
        if (EventManager.Instance.IsTutorial)
        {
            TutorialOnClickNext(); // 다음 대사로 바로 넘겨줌
            foreach (var guide in _buildGuideUI) guide.SetActive(false);
            foreach (var guide in _roadGuideUI) guide.SetActive(false);
            _BackGround.SetActive(true);
            _nextButton.gameObject.SetActive(true); // 다시 화면 클릭 가능하게
            _currentGuideIndex = 0;
            Time.timeScale = 0f;
        }
    }
    
    public void ShowNextBuildGuide()
    {
        if (!EventManager.Instance.IsTutorial) return;
        foreach (var guide in _buildGuideUI) 
            guide.SetActive(false);
        
        if (_currentGuideIndex < _buildGuideUI.Count - 1)
        {
            _currentGuideIndex++;
            _buildGuideUI[_currentGuideIndex].SetActive(true);
        }
    }
    
    public void ShowNextRoadGuide()
    {
        if (!EventManager.Instance.IsTutorial) return;
        foreach (var guide in _roadGuideUI)
            guide.SetActive(false);

        if (_currentGuideIndex < _roadGuideUI.Count - 1)
        {
            _currentGuideIndex++;
            _roadGuideUI[_currentGuideIndex].SetActive(true);
        }
    }

    public void TutorialSkip()
    {
        if (EventManager.Instance.IsTutorial)
        {
            gameObject.SetActive(false);
            EventManager.Instance.IsTutorial = false;
            CameraController cam = FindFirstObjectByType<CameraController>();
            cam.IsCrapting = false;
            _currentGuideIndex = 0;
            Time.timeScale = 1f;
        }
    }
}
