using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System;

public class InBuildingData : MonoBehaviour
{
    [Header("카메라 기준점")]
    public GameObject CameraPivot;

    [Header("캔버스 및 버튼")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _returnButton;

    [Header("건물 정보")]
    public int currentLevel { get; private set; } = 1;
    public int maxLevel { get; private set; } = 2;

    [Header("다음레벨 프리팹")]
    [SerializeField] private List<GameObject> _nextLevelPrefabs;
    [SerializeField] private GameObject _UpgradeExpandArea;

    private FacilityRuntime _facilityRuntime;
    public FacilityRuntime FacilityRuntime => _facilityRuntime;

    [Header("입구/웨이팅/사용 정보")]
    public GameObject EnterPivot;
    public GameObject WaitPivot;
    public GameObject UsePivot;
    public List<GameObject> UsePivots;
    public GameObject FacilityExitPivot;

    public GameObject EntrancePivot;
    public GameObject ExitPivot;
    public List<GameObject> EntranceWayPivots;
    public List<GameObject> ExitWayPivots;

    [Header("내부 그리드 설정")]
    public List<GameObject> _whiteAreaPivots;
    public List<GameObject> _upgradeWhiteAreaPivots;
    private Vector2Int _whiteAreaSize;

    private CameraController _cameraController;

    [Header("시설 이용 공간 설정")]
    [SerializeField] private int _defaultUseCount = 4;
    public int _currentUseCount;
    private List<Transform> _usePivotsTransformsList = new List<Transform>();
    public event Action<List<Transform>> OnUsePivotsChanged;

    [Header("가구 정보")]
    [SerializeField] private FurnitureData _capacityFurnitureData; // 수용성 가구 데이터
    [SerializeField] private FurnitureData _feeFurnitureData;      // 수익성 가구 데이터
    [SerializeField] private int _currentFurnitureCount = 0;
    [SerializeField] private int _maxFurnitureCount = 3;
    
    [Header("수용형 가구 개수 최대치")]
    [SerializeField] private int _maxCapacityFurnitureCount = 3;
    public int _currentCapacityFurnitureCount = 0;
    
    [Header("수익형 가구 개수")]
    public int _currentFeeFurnitureCount = 0;
    
    public FurnitureData CapacityFurnitureData => _capacityFurnitureData;
    public FurnitureData FeeFurnitureData => _feeFurnitureData;
    public int MaxCapacityFurnitureCount => _maxCapacityFurnitureCount;
    public int currentCapacityFurnitureCount => _currentCapacityFurnitureCount;
    public int MaxFurnitureCount => _maxFurnitureCount;
    public int CurrentFurnitureCount => _currentFurnitureCount;

    private void Awake()
    {
        _canvas.gameObject.SetActive(false);
        _whiteAreaSize = new Vector2Int(1, 1);
        _UpgradeExpandArea.SetActive(false);

        if (_currentUseCount < _defaultUseCount)
        {
            _currentUseCount = _defaultUseCount;
        }
    }

    private void Start()
    {
        InitFurnitureData();
        _returnButton.onClick.AddListener(ReturnButton);
        _cameraController = FindFirstObjectByType<CameraController>();
        InBuildingWhiteTilesCreate();

        UpdateUseVisual();
        OnUsePivotsChanged?.Invoke(GetUsePivots());
    }
    // 현재 늘어난 이용 공간을 시각적으로 보여주는 메서드
    public void UpdateUseVisual()
    {
        for (int i = _defaultUseCount; i < UsePivots.Count; i++)
        {
            bool isActive = i < _currentUseCount;
            UsePivots[i].SetActive(isActive);
        }
    }

    // 시설 이용 공간이 늘어날 때 사용하는 메서드
    public void IncreaseUsePivots()
    {
        if (_currentUseCount >= UsePivots.Count)
        {
            return;
        }
        _currentUseCount += _capacityFurnitureData.interiorCapacityGrowth;
        UpdateUseVisual();

        OnUsePivotsChanged?.Invoke(GetUsePivots());
    }

    // 시설 이용 공간이 줄어들 때 사용하는 메서드
    public void DecreaseUsePivots()
    {
        if (_currentUseCount <= _defaultUseCount)
        {
            return;
        }

        _currentUseCount -= _capacityFurnitureData.interiorCapacityGrowth;
        UpdateUseVisual();

        OnUsePivotsChanged?.Invoke(GetUsePivots());
    }

    // 현재 시설 이용 가능 공간들의 Transform 값 반환
    public List<Transform> GetUsePivots()
    {
        _usePivotsTransformsList.Clear();

        for (int i = 0; i < _currentUseCount; i++)
        {
            if (UsePivots[i] != null)
            {
                _usePivotsTransformsList.Add(UsePivots[i].transform);
            }
        }

        return _usePivotsTransformsList;
    }

    // 수용성 가구의 현재 개수를 늘리고 최대치를 초과하면 false 반환
    public bool TryAssignCapacityFurniture()
    {
        if (_currentCapacityFurnitureCount >= _maxCapacityFurnitureCount)
        {
            _currentCapacityFurnitureCount = _maxCapacityFurnitureCount;
            return false;
        }

        if (_currentFurnitureCount >= _maxFurnitureCount)
        {
            _currentFurnitureCount = _maxFurnitureCount;
            return false;
        }

        _currentCapacityFurnitureCount++;
        _currentFurnitureCount++;
        IncreaseUsePivots();
        GoldTest.Instance.PlayerUseMoney(_capacityFurnitureData.interiorPrice);
        Debug.Log($"수용형 가구 설치 개수 : {_currentCapacityFurnitureCount}");
        return true;
    }

    // 수용성 가구의 현재 개수를 줄이는 메서드
    public void RemoveCapacityFurniture()
    {
        _currentCapacityFurnitureCount--;
        _currentFurnitureCount--;

        if (_currentCapacityFurnitureCount <= 0)
        {
            _currentCapacityFurnitureCount = 0;
        }

        if (_currentFurnitureCount <= 0)
        {
            _currentFurnitureCount = 0;
        }
        
        DecreaseUsePivots();
        Debug.Log($"수용형 가구 설치 개수 : {_currentCapacityFurnitureCount}");
    }

    
    public void TryAssignProfitableFurniture()
    {
        if (_currentFeeFurnitureCount > _maxFurnitureCount)
        {
            _currentFeeFurnitureCount = _maxFurnitureCount;
            return;
        }

        if (_currentFurnitureCount > _maxFurnitureCount)
        {
            _currentFurnitureCount = _maxFurnitureCount;
            return;
        }
        
        _currentFeeFurnitureCount++;
        _currentFurnitureCount++;
        
        
        int totalPay = _feeFurnitureData.interiorFeeGrowth * _currentFeeFurnitureCount;
        _facilityRuntime.FurnitureGold = totalPay;
        
        GoldTest.Instance.PlayerUseMoney(_feeFurnitureData.interiorPrice);
        
        Debug.Log($"수익형 가구 개수 {_currentFeeFurnitureCount}, 수익 증가값 : {_feeFurnitureData.interiorFeeGrowth} 총 수익 증가 값 : {totalPay}" );
    }
    
    public void RemoveProfitableFurniture()
    {
        if (_currentFeeFurnitureCount < 0)
        {
            _currentFeeFurnitureCount = 0;
            return;
        }

        if (_currentFurnitureCount < 0)
        {
            _currentFurnitureCount = 0;
            return;
        }
        
        _currentFeeFurnitureCount--;
        _currentFurnitureCount--;
        
        int totalPay = _feeFurnitureData.interiorFeeGrowth * _currentFeeFurnitureCount;
        _facilityRuntime.FurnitureGold = totalPay;
        
        Debug.Log($"수익형 가구 개수 {_currentFeeFurnitureCount}, 수익 증가값 : {_feeFurnitureData.interiorFeeGrowth} 총 수익 증가 값 : {totalPay}" );
        
    }
    
    
    //FurnitureSheetManager로부터 가구들의 정보를 받아오는 메서드 
    public void InitFurnitureData()
    {
        // _facilityRuntime.FacilityID로 대조해서 그에 맞는 가구 정보를 List를 가져옴 
        List<FurnitureData> furnitureDatas =
            FurnitureSheetManager.Instance.GetFurnitureByFacility(_facilityRuntime.FacilityID);

        // BuildType으로 비교하여 가져온 List에서 가져온 데이터를 _capacityFurnitureData, _feeFurnitureData에 대입
        foreach (var data in furnitureDatas)
        {
            switch (data.interiorType)
            {
                case BuildType.CapacityFurniture:
                    _capacityFurnitureData = data;
                    break;
                case BuildType.FeeFurniture:
                    _feeFurnitureData = data;
                    break;
            }
            Debug.Log($"[InBuildingData] 이 가구의 이름은 {data.interiorNameKo} 타입은 {data.interiorType}");
        }
    }

    public void BuildingEntered()
    {
        _canvas.gameObject.SetActive(true);
        GridBuildingSystem.Instance.SetCurrentInBuilding(this);
    }

    public void SetFacilityRuntime(FacilityRuntime facilityRuntime)
    {
        _facilityRuntime = facilityRuntime;
    }
    public void BuildingLevelUp()
    {
        if (maxLevel < currentLevel)
        {
            return;
        }
        
        if (GoldTest.Instance._testGold < FacilityRuntime.UpgradeCost)
        {
            Debug.Log($"골드 부족 업그레이드 불가능");
            return;
        }
        
        if (currentLevel == 1)
        {
            _UpgradeExpandArea.SetActive(true);
            _maxFurnitureCount = 6;

            for (int i = 0; i < _upgradeWhiteAreaPivots.Count; i++)
            {
                _whiteAreaPivots.Add(_upgradeWhiteAreaPivots[i]);
            }
            
            GoldTest.Instance.PlayerUseMoney(FacilityRuntime.UpgradeCost);
            InBuildingWhiteTilesCreate();

            // 일반 시설 -> 고급 시설 ID로 변경
            if (_facilityRuntime != null)
            {
                string upgradedFacilityID = GetUpgradedFacilityID(_facilityRuntime.FacilityID);

                if (string.IsNullOrWhiteSpace(upgradedFacilityID) == false)
                {
                    _facilityRuntime.InitializeFacility(upgradedFacilityID);
                    Debug.Log($"[InBuildingData] 시설 업그레이드 완료 | OldID={_facilityRuntime.FacilityID}, NewID={upgradedFacilityID}");
                }
                else
                {
                    Debug.LogWarning("[InBuildingData] 업그레이드 대상 FacilityID를 만들지 못했습니다.");
                }
            }
        }

        if (currentLevel == 2)
        {
            if (GoldTest.Instance._testGold < FacilityRuntime.UpgradeCost)
            {
                Debug.Log($"골드 부족 업그레이드 불가능");
                return;
            }
            
            GoldTest.Instance.PlayerUseMoney(FacilityRuntime.UpgradeCost);
            
            if (_facilityRuntime != null)
            {
                string upgradedFacilityID = GetUpgradedFacilityID(_facilityRuntime.FacilityID);

                if (string.IsNullOrWhiteSpace(upgradedFacilityID) == false)
                {
                    _facilityRuntime.InitializeFacility(upgradedFacilityID);
                    Debug.Log($"[InBuildingData] 시설 업그레이드 완료 | OldID={_facilityRuntime.FacilityID}, NewID={upgradedFacilityID}");
                }
                else
                {
                    Debug.LogWarning("[InBuildingData] 업그레이드 대상 FacilityID를 만들지 못했습니다.");
                }
            }
            Debug.Log($"이용 요금 증가 {_facilityRuntime.UsageFee}");
        }

        currentLevel++;
        
    }

    private string GetUpgradedFacilityID(string currentFacilityID)
    {
        if (string.IsNullOrWhiteSpace(currentFacilityID))
        {
            return string.Empty;
        }

        int lastUnderscoreIndex = currentFacilityID.LastIndexOf('_');
        if (lastUnderscoreIndex < 0 || lastUnderscoreIndex >= currentFacilityID.Length - 1)
        {
            Debug.LogWarning($"[InBuildingData] FacilityID 형식이 올바르지 않습니다. ID={currentFacilityID}");
            return string.Empty;
        }

        string prefix = currentFacilityID.Substring(0, lastUnderscoreIndex);
        string levelText = currentFacilityID.Substring(lastUnderscoreIndex + 1);

        if (!int.TryParse(levelText, out int currentFacilityLevel))
        {
            Debug.LogWarning($"[InBuildingData] FacilityID 레벨 파싱 실패. ID={currentFacilityID}");
            return string.Empty;
        }

        int nextFacilityLevel = currentFacilityLevel + 1;
        return $"{prefix}_{nextFacilityLevel:00}";
    }

    //public void BuildingLevelUp()
    //{
    //    if (maxLevel < currentLevel)
    //    {
    //        return;
    //    }

    //    if (currentLevel == 1)
    //    {
    //        _UpgradeExpandArea.SetActive(true);

    //        for (int i = 0; i < _upgradeWhiteAreaPivots.Count; i++)
    //        {
    //            _whiteAreaPivots.Add(_upgradeWhiteAreaPivots[i]);
    //        }

    //        InBuildingWhiteTilesCreate();

    //    }


    //    /*
    //    if (currentLevel == 2)
    //    {
    //        _facilityRuntime.UpgradePrice(20);
    //    }

    //    Debug.Log($"<color=yellow>현재 가격 {_facilityRuntime.Gold}</color>");
    //    */

    //    if (_facilityRuntime != null)
    //    {
    //        Debug.Log($"<color=yellow>현재 시설 정보 | ID={_facilityRuntime.FacilityID}, UsageFee={_facilityRuntime.UsageFee}</color>");
    //    }

    //    currentLevel++;
    //}

    public void ReturnButton()
    {
        if (UIManager.Instance.OpenMenu == true) return;

        _cameraController.ReturnToWorld();
        _canvas.gameObject.SetActive(false);

        if (_facilityRuntime != null)
        {
            UIManager.Instance.SetFurnitureButtonActive(_facilityRuntime.FacilityType, false);
        }

        UIManager.Instance._buildButton.SetActive(true);
        GridBuildingSystem.Instance.SetCurrentInBuilding(null);

        UIManager.Instance.OpenMenu = false;
        UIManager.Instance._topUIPanel.SetActive(true);
        Time.timeScale = 1f;
    }

    public void InBuildingWhiteTilesCreate()
    {
        if (GridBuildingSystem.Instance.MainTilemap == null)
        {
            return;
        }

        TileBase whiteTile = Resources.Load<TileBase>("SGH_Test/white");

        foreach (var pivot in _whiteAreaPivots)
        {
            Vector3Int cellPos = GridBuildingSystem.Instance.MainTilemap.WorldToCell(pivot.transform.position);

            for (int x = 0; x < _whiteAreaSize.x; x++)
            {
                for (int y = 0; y < _whiteAreaSize.y; y++)
                {
                    Vector3Int pos = new Vector3Int(cellPos.x + x, cellPos.y + y, 0);
                    TileBase currentTile = GridBuildingSystem.Instance.MainTilemap.GetTile(pos);

                    if (currentTile == whiteTile)
                    {
                        continue;
                    }

                    GridBuildingSystem.Instance.MainTilemap.SetTile(pos, whiteTile);
                }
            }
        }
    }

    // 세이브 로드 관련
    public void SetLevelPrice(int level, int price)
    {
        currentLevel = level;

        if (currentLevel >= 2)
        {
            _UpgradeExpandArea.SetActive(true);
            _maxFurnitureCount = 6;

            for (int i = 0; i < _upgradeWhiteAreaPivots.Count; i++)
            {
                _whiteAreaPivots.Add(_upgradeWhiteAreaPivots[i]);
            }
            
            if (_facilityRuntime != null)
            {
                string upgradedFacilityID = GetUpgradedFacilityID(_facilityRuntime.FacilityID);

                if (string.IsNullOrWhiteSpace(upgradedFacilityID) == false)
                {
                    _facilityRuntime.InitializeFacility(upgradedFacilityID);
                    Debug.Log($"[InBuildingData] 시설 업그레이드 완료 | OldID={_facilityRuntime.FacilityID}, NewID={upgradedFacilityID}");
                }
                else
                {
                    Debug.LogWarning("[InBuildingData] 업그레이드 대상 FacilityID를 만들지 못했습니다.");
                }
            }
        }
        if (currentLevel >= 3)
        {
            if (_facilityRuntime != null)
            {
                string upgradedFacilityID = GetUpgradedFacilityID(_facilityRuntime.FacilityID);

                if (string.IsNullOrWhiteSpace(upgradedFacilityID) == false)
                {
                    _facilityRuntime.InitializeFacility(upgradedFacilityID);
                    Debug.Log($"[InBuildingData] 시설 업그레이드 완료 | OldID={_facilityRuntime.FacilityID}, NewID={upgradedFacilityID}");
                }
                else
                {
                    Debug.LogWarning("[InBuildingData] 업그레이드 대상 FacilityID를 만들지 못했습니다.");
                }
            }
            Debug.Log($"이용 요금 증가 {_facilityRuntime.UsageFee}");
        }


        _facilityRuntime.FurnitureGold = price;


        Debug.Log("[InBuildingData] SetLevelPrice의 price 직접 대입 기능은 사용하지 않습니다. 가격은 SO 기준입니다.");
        InBuildingWhiteTilesCreate();
    }

    public void SetCurrentCount(int useCount, int furnitureCount, int capacityFurnitureCount)
    {
        _currentUseCount = useCount;
        _currentFeeFurnitureCount = furnitureCount;
        _currentCapacityFurnitureCount = capacityFurnitureCount;
        _currentFurnitureCount = furnitureCount + capacityFurnitureCount;
    }
}