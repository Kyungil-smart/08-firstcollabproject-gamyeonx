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
    public GameObject _restaurantBuildButton;
    public GameObject _hotSpringBuildButton;
    public GameObject _trainingGround_Build_Button;
    public GameObject _shopBuildButton;
    public GameObject _vendingMachineBuildButton;
    [SerializeField] private GameObject _buildPanel;
    [SerializeField] private GameObject _roadSubScrollView;
    [SerializeField] private GameObject _buildSubScrollView;
    public GameObject _restaurantBuildPanel;
    public GameObject _hotSpringBuildPanel;
    public GameObject _trainingGroundBuildPanel;
    public GameObject _shopBuildPanel;
    public GameObject _vendingMachineBuildPanel;
    public GameObject _topUIPanel;
    [SerializeField] private GameObject _roadTouchUIPanel;
    [SerializeField] private GameObject _roadRemoveTouchUIPanel;
    public GameObject _restaurantRemoveTouchUIPanel;
    public GameObject _hotSpringRemoveTouchUIPanel;
    public GameObject _trainingGroundRemoveTouchUIPanel;
    public GameObject _shopRemoveTouchUIPanel;
    public GameObject _vendingMachineRemoveTouchUIPanel;
    public GameObject _buildingRemoveTouchUIPanel;
    public GameObject _restaurantTouchUIPanel;
    public GameObject _hotSpringTouchUIPanel;
    public GameObject _trainingGroundTouchUIPanel;
    public GameObject _shopTouchUIPanel;
    public GameObject _vendingMachineTouchUIPanel;
    [SerializeField] private GameObject _roadDemolishUIPanel;
    public GameObject _restaurantDemolishUIPanel;
    public GameObject _hotSpringDemolishUIPanel;
    public GameObject _trainingGroundDemolishUIPanel;
    public GameObject _shopDemolishUIPanel;
    public GameObject _vendingMachineBuildPanelDemolishUIPanel;
    [SerializeField] private GameObject _buildTouchUIPanel;
    [SerializeField] private GameObject _RoadDemolishdoublecheckPanel;
    [SerializeField] private GameObject _RestaurantDemolishdoublecheckPanel;
    [SerializeField] private GameObject _VendingMachineDemolishdoublecheckPanel;
    [SerializeField] private GameObject _ShopDemolishdoublecheckPanel;
    [SerializeField] private GameObject _TrainingGroundDemolishdoublecheckPanel;
    [SerializeField] private GameObject _HotSpringDemolishdoublecheckPanel;
    [SerializeField] private GridBuildingSystem _gridBuildingSystem;
    public bool OpenMenu = false;

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
        Time.timeScale = 1f;
    }

    //=================여기서 부터는 UI관련 함수들==========================
    //=================빌드 버튼
    public void OnClickBuildButton()
    {
        _buildButton.SetActive(false);
        _topUIPanel.SetActive(false);
        _buildPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnClickBuildBackButton()
    {
        _buildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        _buildPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
        Time.timeScale = 1f;
    }

    public void OnClickBuildRoadSubButton()
    {
        _roadSubScrollView.SetActive(true);
        _buildSubScrollView.SetActive(false);
    }

    public void OnClickBuildBuildSubButton()
    {
        _roadSubScrollView.SetActive(false);
        _buildSubScrollView.SetActive(true);
    }

    public void OnClickBuildRoadObjectButton()
    {
        OpenMenu = true;
        _roadTouchUIPanel.SetActive(true);
        _buildPanel.SetActive(false);
    }
    public void OnClickBuildBuildObjectButton()
    {
        if (_gridBuildingSystem.GoldAmount > GoldTest.Instance._testGold)
        {
            Debug.Log($"[BuildTouchUI]골드가 부족합니다 {_gridBuildingSystem.GoldAmount}");
            return;
        }

        if (_gridBuildingSystem.UnlockRevenue > GoldTest.Instance.IncreasedGold)
        {
            Debug.Log($"[BuildTouchUI]누적 비용이 부족합니다 {_gridBuildingSystem.UnlockRevenue}");
            return;
        }
        OpenMenu = true;
        _buildTouchUIPanel.SetActive(true);
        _buildPanel.SetActive(false);
    }
    //==============길 설치용 터치패널
    public void OnClickRoadTouchUIPlace()
    {
        _roadTouchUIPanel.SetActive(false);
        _buildPanel.SetActive(true);
        OpenMenu = false;
    }

    public void OnClickRoadTouchUICancel()
    {
        _roadTouchUIPanel.SetActive(false);
        _buildPanel.SetActive(true);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
        OpenMenu = false;
    }

    public void OnClickRoadTouchUIBack()
    {
        _roadTouchUIPanel.SetActive(false);
        _buildPanel.SetActive(true);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
        OpenMenu = false;
    }

    //=================길 삭제용 패널

    public void OnClickRoadDemolishUIReposition()
    {
        _roadDemolishUIPanel.SetActive(false);
        _roadRemoveTouchUIPanel.SetActive(true);
    }

    public void OnClickRoadDemolishUIDemolish()
    {
        _roadDemolishUIPanel.SetActive(false);
        // 치명적 버그 발견
        //_RoadDemolishdoublecheckPanel.SetActive(true);
        OpenMenu = false;
    }

    public void OnClickRoadDemolishUIBack()
    {
        _roadDemolishUIPanel.SetActive(false);
        OpenMenu = false;
    }

    public void OnClickRoadDemolishYes()
    {
        _RoadDemolishdoublecheckPanel.SetActive(false);
        OpenMenu = false;
    }
    public void OnClickRoadDemolishNo()
    {  
        _roadDemolishUIPanel.SetActive(true);
        _RoadDemolishdoublecheckPanel.SetActive(false);
    }

    //===============길 재이동, 재배치

    public void OnClickRemoveTouchUIPlace()
    {
        _roadRemoveTouchUIPanel.SetActive(false);
        OpenMenu = false;
    }

    public void OnClickRemoveTouchUIbackup()
    {
        _roadRemoveTouchUIPanel.SetActive(false);
        OpenMenu = false;
    }

    public void OnClickRemoveTouchUICancel()
    {
        _roadRemoveTouchUIPanel.SetActive(false);
        _roadDemolishUIPanel.SetActive(true);
    }

    public void OnClickRemoveTouchUIBack()
    {
        _roadRemoveTouchUIPanel.SetActive(false);
        _roadDemolishUIPanel.SetActive(true);
    }

    //================== 건물 설치용 터치
    public void OnClickBuildTouchUIPlace()
    {
        _buildTouchUIPanel.SetActive(false);
        _buildPanel.SetActive(true);
        OpenMenu = false;
    }
    public void OnClickBuildTouchUICancel()
    {
        _buildTouchUIPanel.SetActive(false);
        _buildPanel.SetActive(true);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
        OpenMenu = false;
    }

    public void OnClickBuildTouchUIBack()
    {
        _buildTouchUIPanel.SetActive(false);
        _buildPanel.SetActive(true);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
        OpenMenu = false;
    }

    //================= 건물 재이동용
    public void OnClickBuildRemoveUISetup()
    {
        _buildingRemoveTouchUIPanel.SetActive(true);
    }

    public void OnClickBuildRemoveUIPlace()
    {
        _buildingRemoveTouchUIPanel.SetActive(false);
    }

    public void OnClickBuildRemoveUIbackup()
    {
        _buildingRemoveTouchUIPanel.SetActive(false);
    }

    public void OnClickBuildRemoveUICancel()
    {
        _buildingRemoveTouchUIPanel.SetActive(false);
    }

    public void OnClickBuildRemoveUIBack()
    {
        _buildingRemoveTouchUIPanel.SetActive(false);
    }

    //=========================== 레스토랑용

    public void OnClickRestaurantBuildButton()
    {
        OpenMenu = true;
        _restaurantBuildPanel.SetActive(true);
        _restaurantBuildButton.SetActive(false);
        Time.timeScale = 0f;
    }

    public void OnClickRestaurantBuildObjectButton()
    {
        _restaurantBuildPanel.SetActive(false);
        _topUIPanel.SetActive(false);
        _restaurantTouchUIPanel.SetActive(true);
    }

    public void OnClickRestaurantBuildBackButton()
    {
        OpenMenu = false;
        _restaurantBuildPanel.SetActive(false);
        _restaurantBuildButton.SetActive(true);
        Time.timeScale = 1f;
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    //====== 레스토랑 건설 터치

    public void OnClickRestaurantBuildTouchUIPlace()
    {
        OpenMenu = false;
        _restaurantBuildPanel.SetActive(true);
        _restaurantTouchUIPanel.SetActive(false);
    }

    public void OnClickRestaurantBuildTouchUICancel()
    {
        OpenMenu = false;
        _restaurantBuildPanel.SetActive(true);
        _restaurantTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    public void OnClickRestaurantBuildTouchUIBack()
    {
        OpenMenu = false;
        _restaurantBuildPanel.SetActive(true);
        _restaurantTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    //===================== 레스토랑 삭제 터치패널

    public void OnClickRestaurantDemolishUIReposition()
    {
        _restaurantDemolishUIPanel.SetActive(false);
        _restaurantRemoveTouchUIPanel.SetActive(true);
    }
    public void OnClickRestaurantDemolishUIDemolish()
    {
        _restaurantDemolishUIPanel.SetActive(false);
        _RestaurantDemolishdoublecheckPanel.SetActive(true);
    }
    public void OnClickRestaurantDemolishUIBack()
    {
        OpenMenu = false;
        _restaurantDemolishUIPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _restaurantBuildButton.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickRestaurantDemolishdoublecheckYes()
    {
        OpenMenu = false;
        _RestaurantDemolishdoublecheckPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _restaurantBuildButton.SetActive(true);
        Time.timeScale = 1;
    }
    public void OnClickRestaurantDemolishdoublecheckNo()
    {
        _RestaurantDemolishdoublecheckPanel.SetActive(false);
        _restaurantDemolishUIPanel.SetActive(true);
    }
}
