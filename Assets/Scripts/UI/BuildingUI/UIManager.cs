using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public bool IsStop = false; // 일시정지 판정

    [Header("각 건물의 건축 버튼")]
    [SerializeField] private List<FurnitureButtonData> _furnitureButtons;
    [Header("메인 건축 버튼")]
    public GameObject _buildButton;
    [Header("건물 삭제 재확인 오브젝트")]
    public GameObject BuildDoublecheckPanel;

    [Header("그 외")]
    public GoldTest _goldTest;
    public GameTime _gameTime;
    
    public HashSet<string> _triggeredEvents = new HashSet<string>();

    [System.Serializable]
    public class FurnitureButtonData
    {
        public EFacilityType type;
        public GameObject button;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        _goldTest = GetComponent<GoldTest>();
        _gameTime = GetComponent<GameTime>();
        //DontDestroyOnLoad(gameObject);
    }

    public void SetFurnitureButtonActive(EFacilityType type, bool active)
    {
        var data = _furnitureButtons.Find(x => x.type == type);
        if (data != null && data.button != null)
            data.button.SetActive(active);
    }


    // 건물 삭제 확인 팝업 관련... 프리펩에 접근하기위해서
    public void OnClickSetBuildDoublecheckPanel()
    {
        BuildDoublecheckPanel.SetActive(true);
    }
    public void OnClickNoBuildDoublecheckPanel()
    {
        BuildDoublecheckPanel.SetActive(false);
    }
    public void OnClickCloseBuildDoublecheckPanel()
    {
        BuildDoublecheckPanel.SetActive(false);
    }

    public void OnClickDemolishYes()
    {
        GridBuildingSystem.Instance.DeleteSelectedBuilding();
    }
}
