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

    [Header("모든 UI 최대한 등록")]
    [SerializeField] private GameObject _restaurantBuildButton;
    [SerializeField] private GameObject _hotSpringBuildButton;
    [SerializeField] private GameObject _trainingGround_Build_Button;
    [SerializeField] private GameObject _shopBuildButton;
    [SerializeField] private GameObject _vendingMachineBuildButton;
    [SerializeField] private GameObject _buildPanel;
    [SerializeField] private GameObject _roadSubScrollView;
    [SerializeField] private GameObject _buildSubScrollView;
    [SerializeField] private GameObject _restaurantBuildPanel;
    [SerializeField] private GameObject _hotSpringBuildPanel;
    [SerializeField] private GameObject _trainingGroundBuildPanel;
    [SerializeField] private GameObject _shopBuildPanel;
    [SerializeField] private GameObject _vendingMachineBuildPanel;
    [SerializeField] private GameObject _topUIPanel;
    [SerializeField] private GameObject _roadTouchUIPanel;
    [SerializeField] private GameObject _roadRemoveTouchUIPanel;
    [SerializeField] private GameObject _restaurantRemoveTouchUIPanel;
    [SerializeField] private GameObject _hotSpringRemoveTouchUIPanel;
    [SerializeField] private GameObject _trainingGroundRemoveTouchUIPanel;
    [SerializeField] private GameObject _shopRemoveTouchUIPanel;
    [SerializeField] private GameObject _vendingMachineRemoveTouchUIPanel;
    [SerializeField] private GameObject _buildingRemoveTouchUIPanel;
    [SerializeField] private GameObject _restaurantTouchUIPanel;
    [SerializeField] private GameObject _hotSpringTouchUIPanel;
    [SerializeField] private GameObject _trainingGroundTouchUIPanel;
    [SerializeField] private GameObject _shopTouchUIPanel;
    [SerializeField] private GameObject _vendingMachineTouchUIPanel;
    [SerializeField] private GameObject _roadDemolishUIPanel;
    [SerializeField] private GameObject _restaurantDemolishUIPanel;
    [SerializeField] private GameObject _hotSpringDemolishUIPanel;
    [SerializeField] private GameObject _trainingGroundDemolishUIPanel;
    [SerializeField] private GameObject _shopDemolishUIPanel;
    [SerializeField] private GameObject _vendingMachineBuildPanelDemolishUIPanel;
    [SerializeField] private GameObject _buildTouchUIPanel;
    [SerializeField] private GameObject _buildDemolishdoublecheckPanel;

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

    //=================여기서 부터는 UI관련 함수들==========================

    //public void OnClickBuild
}
