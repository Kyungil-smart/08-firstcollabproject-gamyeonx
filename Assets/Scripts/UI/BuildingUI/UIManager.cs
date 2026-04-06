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
        _RoadDemolishdoublecheckPanel.SetActive(true);
    }

    public void OnClickRoadDemolishUIBack()
    {
        _roadDemolishUIPanel.SetActive(false);
        _buildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        OpenMenu = false;
        Time.timeScale = 1;
    }

    public void OnClickRoadDemolishYes()
    {
        _RoadDemolishdoublecheckPanel.SetActive(false);
        OpenMenu = false;
        Time.timeScale = 1;
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
        if (_gridBuildingSystem.GoldAmount > GoldTest.Instance._testGold)
        {
            Debug.Log($"[BuildTouchUI]골드가 부족합니다 {_gridBuildingSystem.GoldAmount}");
            return;
        }

        if (!_gridBuildingSystem.IsCanPlacing)
        {
            Debug.Log("[BuildTouchUI]수용성 가구가 3개를 초과했습니다.");
            return;
        }

        if (_gridBuildingSystem.CurrentFurnitureCount >= _gridBuildingSystem.MaxFurnitureCount)
        {
            Debug.Log($"[UIManagers]가구 배치 불가능 / 가구가 {_gridBuildingSystem.CurrentFurnitureCount}개 입니다.");
            return;
        }
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
        _restaurantBuildPanel.SetActive(true);
        _restaurantTouchUIPanel.SetActive(false);
    }

    public void OnClickRestaurantBuildTouchUICancel()
    {
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

    //======================= 레스토랑 재이동 

    public void OnClickRestaurantRemoveUIPlace()
    {
        OpenMenu = false;
        _restaurantRemoveTouchUIPanel.SetActive(false);
        _restaurantBuildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickRestaurantRemoveUIbackup()
    {
        OpenMenu = false;
        _restaurantRemoveTouchUIPanel.SetActive(false);
        _restaurantBuildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickRestaurantRemoveUICancel()
    {
        _restaurantRemoveTouchUIPanel.SetActive(false);
        _restaurantDemolishUIPanel.SetActive(true);
    }

    public void OnClickRestaurantRemoveUIBack()
    {
        _restaurantRemoveTouchUIPanel.SetActive(false);
        _restaurantDemolishUIPanel.SetActive(true);
    }

    //=========================== 온천용

    public void OnClickHotSpringBuildButton()
    {
        OpenMenu = true;
        _hotSpringBuildPanel.SetActive(true);
        _hotSpringBuildButton.SetActive(false);
        Time.timeScale = 0f;
    }

    public void OnClickHotSpringBuildObjectButton()
    {
        if (_gridBuildingSystem.GoldAmount > GoldTest.Instance._testGold)
        {
            Debug.Log($"[BuildTouchUI]골드가 부족합니다 {_gridBuildingSystem.GoldAmount}");
            return;
        }

        if (!_gridBuildingSystem.IsCanPlacing)
        {
            Debug.Log("[BuildTouchUI]수용성 가구가 3개를 초과했습니다.");
            return;
        }
        
        if (_gridBuildingSystem.CurrentFurnitureCount >= _gridBuildingSystem.MaxFurnitureCount)
        {
            Debug.Log($"[UIManagers]가구 배치 불가능 / 가구가 {_gridBuildingSystem.CurrentFurnitureCount}개 입니다.");
            return;
        }
        _hotSpringBuildPanel.SetActive(false);
        _topUIPanel.SetActive(false);
        _hotSpringTouchUIPanel.SetActive(true);
    }

    public void OnClickHotSpringBuildBackButton()
    {
        OpenMenu = false;
        _hotSpringBuildPanel.SetActive(false);
        _hotSpringBuildButton.SetActive(true);
        Time.timeScale = 1f;
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    //====== 온천 건설 터치

    public void OnClickHotSpringBuildTouchUIPlace()
    {
        _hotSpringBuildPanel.SetActive(true);
        _hotSpringTouchUIPanel.SetActive(false);
    }

    public void OnClickHotSpringBuildTouchUICancel()
    {
        _hotSpringBuildPanel.SetActive(true);
        _hotSpringTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    public void OnClickHotSpringBuildTouchUIBack()
    {
        _hotSpringBuildPanel.SetActive(true);
        _hotSpringTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    //===================== 온천 삭제 터치패널

    public void OnClickHotSpringDemolishUIReposition()
    {
        _hotSpringDemolishUIPanel.SetActive(false);
        _hotSpringRemoveTouchUIPanel.SetActive(true);
    }
    public void OnClickHotSpringDemolishUIDemolish()
    {
        _hotSpringDemolishUIPanel.SetActive(false);
        _HotSpringDemolishdoublecheckPanel.SetActive(true);
    }
    public void OnClickHotSpringDemolishUIBack()
    {
        OpenMenu = false;
        _hotSpringDemolishUIPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _hotSpringBuildButton.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickHotSpringDemolishdoublecheckYes()
    {
        OpenMenu = false;
        _HotSpringDemolishdoublecheckPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _hotSpringBuildButton.SetActive(true);
        Time.timeScale = 1;
    }
    public void OnClickHotSpringDemolishdoublecheckNo()
    {
        _HotSpringDemolishdoublecheckPanel.SetActive(false);
        _hotSpringDemolishUIPanel.SetActive(true);
    }

    //======================= 온천 재이동 

    public void OnClickHotSpringRemoveUIPlace()
    {
        OpenMenu = false;
        _hotSpringRemoveTouchUIPanel.SetActive(false);
        _hotSpringBuildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickHotSpringRemoveUIbackup()
    {
        OpenMenu = false;
        _hotSpringRemoveTouchUIPanel.SetActive(false);
        _hotSpringBuildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickHotSpringRemoveUICancel()
    {
        _hotSpringRemoveTouchUIPanel.SetActive(false);
        _hotSpringDemolishUIPanel.SetActive(true);
    }

    public void OnClickHotSpringRemoveUIBack()
    {
        _hotSpringRemoveTouchUIPanel.SetActive(false);
        _hotSpringDemolishUIPanel.SetActive(true);
    }
    //=========================== 훈련소용

    public void OnClickTrainingGroundBuildButton()
    {
        OpenMenu = true;
        _trainingGroundBuildPanel.SetActive(true);
        _trainingGround_Build_Button.SetActive(false);
        Time.timeScale = 0f;
    }

    public void OnClickTrainingGroundBuildObjectButton()
    {
        if (_gridBuildingSystem.GoldAmount > GoldTest.Instance._testGold)
        {
            Debug.Log($"[BuildTouchUI]골드가 부족합니다 {_gridBuildingSystem.GoldAmount}");
            return;
        }

        if (!_gridBuildingSystem.IsCanPlacing)
        {
            Debug.Log("[BuildTouchUI]수용성 가구가 3개를 초과했습니다.");
            return;
        }
        
        if (_gridBuildingSystem.CurrentFurnitureCount >= _gridBuildingSystem.MaxFurnitureCount)
        {
            Debug.Log($"[UIManagers]가구 배치 불가능 / 가구가 {_gridBuildingSystem.CurrentFurnitureCount}개 입니다.");
            return;
        }
        _trainingGroundBuildPanel.SetActive(false);
        _topUIPanel.SetActive(false);
        _trainingGroundTouchUIPanel.SetActive(true);
    }

    public void OnClickTrainingGroundBuildBackButton()
    {
        OpenMenu = false;
        _trainingGroundBuildPanel.SetActive(false);
        _trainingGround_Build_Button.SetActive(true);
        Time.timeScale = 1f;
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    //====== 훈련소 건설 터치

    public void OnClickTrainingGroundBuildTouchUIPlace()
    {
        _trainingGroundBuildPanel.SetActive(true);
        _trainingGroundTouchUIPanel.SetActive(false);
    }

    public void OnClickTrainingGroundBuildTouchUICancel()
    {
        _trainingGroundBuildPanel.SetActive(true);
        _trainingGroundTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    public void OnClickTrainingGroundBuildTouchUIBack()
    {
        _trainingGroundBuildPanel.SetActive(true);
        _trainingGroundTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }
    //===================== 훈련소 삭제 터치패널

    public void OnClickTrainingGroundDemolishUIReposition()
    {
        _trainingGroundDemolishUIPanel.SetActive(false);
        _trainingGroundRemoveTouchUIPanel.SetActive(true);
    }
    public void OnClickTrainingGroundDemolishUIDemolish()
    {
        _trainingGroundDemolishUIPanel.SetActive(false);
        _TrainingGroundDemolishdoublecheckPanel.SetActive(true);
    }
    public void OnClickTrainingGroundDemolishUIBack()
    {
        OpenMenu = false;
        _trainingGroundDemolishUIPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _trainingGround_Build_Button.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickTrainingGroundDemolishdoublecheckYes()
    {
        OpenMenu = false;
        _TrainingGroundDemolishdoublecheckPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _trainingGround_Build_Button.SetActive(true);
        Time.timeScale = 1;
    }
    public void OnClickTrainingGroundDemolishdoublecheckNo()
    {
        _TrainingGroundDemolishdoublecheckPanel.SetActive(false);
        _trainingGroundDemolishUIPanel.SetActive(true);
    }

    //======================= 훈련소 재이동 

    public void OnClickTrainingGroundRemoveUIPlace()
    {
        OpenMenu = false;
        _trainingGroundRemoveTouchUIPanel.SetActive(false);
        _trainingGround_Build_Button.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickTrainingGroundRemoveUIbackup()
    {
        OpenMenu = false;
        _trainingGroundRemoveTouchUIPanel.SetActive(false);
        _trainingGround_Build_Button.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickTrainingGroundRemoveUICancel()
    {
        _trainingGroundRemoveTouchUIPanel.SetActive(false);
        _trainingGroundDemolishUIPanel.SetActive(true);
    }

    public void OnClickTrainingGroundRemoveUIBack()
    {
        _trainingGroundRemoveTouchUIPanel.SetActive(false);
        _trainingGroundDemolishUIPanel.SetActive(true);
    }
    //=========================== 숍용

    public void OnClickShopBuildButton()
    {
        OpenMenu = true;
        _shopBuildPanel.SetActive(true);
        _shopBuildButton.SetActive(false);
        Time.timeScale = 0f;
    }

    public void OnClickShopBuildObjectButton()
    {
        if (_gridBuildingSystem.GoldAmount > GoldTest.Instance._testGold)
        {
            Debug.Log($"[BuildTouchUI]골드가 부족합니다 {_gridBuildingSystem.GoldAmount}");
            return;
        }

        if (!_gridBuildingSystem.IsCanPlacing)
        {
            Debug.Log("[BuildTouchUI]수용성 가구가 3개를 초과했습니다.");
            return;
        }
        
        if (_gridBuildingSystem.CurrentFurnitureCount >= _gridBuildingSystem.MaxFurnitureCount)
        {
            Debug.Log($"[UIManagers]가구 배치 불가능 / 가구가 {_gridBuildingSystem.CurrentFurnitureCount}개 입니다.");
            return;
        }
        _shopBuildPanel.SetActive(false);
        _topUIPanel.SetActive(false);
        _shopTouchUIPanel.SetActive(true);
    }

    public void OnClickShopBuildBackButton()
    {
        OpenMenu = false;
        _shopBuildPanel.SetActive(false);
        _shopBuildButton.SetActive(true);
        Time.timeScale = 1f;
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    //====== 숍 건설 터치

    public void OnClickShopBuildTouchUIPlace()
    {
        _shopBuildPanel.SetActive(true);
        _shopTouchUIPanel.SetActive(false);
    }

    public void OnClickShopBuildTouchUICancel()
    {
        _shopBuildPanel.SetActive(true);
        _shopTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    public void OnClickShopBuildTouchUIBack()
    {
        _shopBuildPanel.SetActive(true);
        _shopTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }
    //===================== 숍 삭제 터치패널

    public void OnClickShopDemolishUIReposition()
    {
        _shopDemolishUIPanel.SetActive(false);
        _shopRemoveTouchUIPanel.SetActive(true);
    }
    public void OnClickShopDemolishUIDemolish()
    {
        _shopDemolishUIPanel.SetActive(false);
        _ShopDemolishdoublecheckPanel.SetActive(true);
    }
    public void OnClickShopDemolishUIBack()
    {
        OpenMenu = false;
        _shopDemolishUIPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _shopBuildButton.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickShopDemolishdoublecheckYes()
    {
        OpenMenu = false;
        _ShopDemolishdoublecheckPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _shopBuildButton.SetActive(true);
        Time.timeScale = 1;
    }
    public void OnClickShopDemolishdoublecheckNo()
    {
        _ShopDemolishdoublecheckPanel.SetActive(false);
        _shopDemolishUIPanel.SetActive(true);
    }

    //======================= 숍 재이동 

    public void OnClickShopRemoveUIPlace()
    {
        OpenMenu = false;
        _shopRemoveTouchUIPanel.SetActive(false);
        _shopBuildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickShopRemoveUIbackup()
    {
        OpenMenu = false;
        _shopRemoveTouchUIPanel.SetActive(false);
        _shopBuildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickShopRemoveUICancel()
    {
        _shopRemoveTouchUIPanel.SetActive(false);
        _shopDemolishUIPanel.SetActive(true);
    }

    public void OnClickShopRemoveUIBack()
    {
        _shopRemoveTouchUIPanel.SetActive(false);
        _shopDemolishUIPanel.SetActive(true);
    }
    //=========================== 카폐용

    public void OnClickVendingMachineBuildButton()
    {
        OpenMenu = true;
        _vendingMachineBuildPanel.SetActive(true);
        _vendingMachineBuildButton.SetActive(false);
        Time.timeScale = 0f;
    }

    public void OnClickVendingMachineBuildObjectButton()
    {
        if (_gridBuildingSystem.GoldAmount > GoldTest.Instance._testGold)
        {
            Debug.Log($"[BuildTouchUI]골드가 부족합니다 {_gridBuildingSystem.GoldAmount}");
            return;
        }

        if (!_gridBuildingSystem.IsCanPlacing)
        {
            Debug.Log("[BuildTouchUI]수용성 가구가 3개를 초과했습니다.");
            return;
        }
        
        if (_gridBuildingSystem.CurrentFurnitureCount >= _gridBuildingSystem.MaxFurnitureCount)
        {
            Debug.Log($"[UIManagers]가구 배치 불가능 / 가구가 {_gridBuildingSystem.CurrentFurnitureCount}개 입니다.");
            return;
        }
        
        _vendingMachineBuildPanel.SetActive(false);
        _topUIPanel.SetActive(false);
        _vendingMachineTouchUIPanel.SetActive(true);
    }

    public void OnClickVendingMachineBuildBackButton()
    {
        OpenMenu = false;
        _vendingMachineBuildPanel.SetActive(false);
        _vendingMachineBuildButton.SetActive(true);
        Time.timeScale = 1f;
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    //====== 카폐 건설 터치

    public void OnClickVendingMachineBuildTouchUIPlace()
    {
        _vendingMachineBuildPanel.SetActive(true);
        _vendingMachineTouchUIPanel.SetActive(false);
    }

    public void OnClickVendingMachineBuildTouchUICancel()
    {
        _vendingMachineBuildPanel.SetActive(true);
        _vendingMachineTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }

    public void OnClickVendingMachineBuildTouchUIBack()
    {
        _vendingMachineBuildPanel.SetActive(true);
        _vendingMachineTouchUIPanel.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
    }
    //===================== 카폐 삭제 터치패널

    public void OnClickVendingMachineDemolishUIReposition()
    {
        _vendingMachineBuildPanelDemolishUIPanel.SetActive(false);
        _vendingMachineRemoveTouchUIPanel.SetActive(true);
    }
    public void OnClickVendingMachineDemolishUIDemolish()
    {
        _vendingMachineBuildPanelDemolishUIPanel.SetActive(false);
        _VendingMachineDemolishdoublecheckPanel.SetActive(true);
    }
    public void OnClickVendingMachineDemolishUIBack()
    {
        OpenMenu = false;
        _vendingMachineBuildPanelDemolishUIPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _vendingMachineBuildButton.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickVendingMachineDemolishdoublecheckYes()
    {
        OpenMenu = false;
        _VendingMachineDemolishdoublecheckPanel.SetActive(false);
        _topUIPanel.SetActive(true);
        _vendingMachineBuildButton.SetActive(true);
        Time.timeScale = 1;
    }
    public void OnClickVendingMachineDemolishdoublecheckNo()
    {
        _VendingMachineDemolishdoublecheckPanel.SetActive(false);
        _vendingMachineBuildPanelDemolishUIPanel.SetActive(true);
    }

    //======================= 카폐 재이동 

    public void OnClickVendingMachineRemoveUIPlace()
    {
        OpenMenu = false;
        _vendingMachineRemoveTouchUIPanel.SetActive(false);
        _vendingMachineBuildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickVendingMachineRemoveUIbackup()
    {
        OpenMenu = false;
        _vendingMachineRemoveTouchUIPanel.SetActive(false);
        _vendingMachineBuildButton.SetActive(true);
        _topUIPanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void OnClickVendingMachineRemoveUICancel()
    {
        _vendingMachineRemoveTouchUIPanel.SetActive(false);
        _vendingMachineBuildPanelDemolishUIPanel.SetActive(true);
    }

    public void OnClickVendingMachineRemoveUIBack()
    {
        _vendingMachineRemoveTouchUIPanel.SetActive(false);
        _vendingMachineBuildPanelDemolishUIPanel.SetActive(true);
    }
}
