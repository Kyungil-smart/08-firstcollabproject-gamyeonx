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

    [Header("수용형 가구 개수 최대치")]
    [SerializeField] private int _maxCapacityFurnitureCount = 3;
    [SerializeField] private int _currentCapacityFurnitureCount = 0;
    public int currentCapacityFurnitureCount => _currentCapacityFurnitureCount;

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
        _returnButton.onClick.AddListener(ReturnButton);
        _cameraController = FindFirstObjectByType<CameraController>();
        InBuildingWhiteTilesCreate();

        UpdateUseVisual();
        OnUsePivotsChanged?.Invoke(GetUsePivots());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            IncreaseUsePivots();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            DecreaseUsePivots();
        }
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

        _currentUseCount++;
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

        _currentUseCount--;
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

        _currentCapacityFurnitureCount++;
        IncreaseUsePivots();
        Debug.Log($"수용형 가구 설치 개수 : {_currentCapacityFurnitureCount}");
        return true;
    }

    // 수용성 가구의 현재 개수를 줄이는 메서드
    public void RemoveCapacityFurniture()
    {
        _currentCapacityFurnitureCount--;

        if (_currentCapacityFurnitureCount <= 0)
        {
            _currentCapacityFurnitureCount = 0;
        }

        DecreaseUsePivots();
        Debug.Log($"수용형 가구 설치 개수 : {_currentCapacityFurnitureCount}");
    }


    // 비활성 예정
    public void TryAssignProfitableFurniture()
    {
        Debug.Log("[InBuildingData] 수익형 가구 가격 증가 테스트 기능은 현재 사용하지 않습니다.");

        /*
        _facilityRuntime.UpgradePrice(30);
        */
    }

    //비활성 예정
    public void RemoveProfitableFurniture()
    {
        Debug.Log("[InBuildingData] 수익형 가구 가격 감소 테스트 기능은 현재 사용하지 않습니다.");

        /*
        _facilityRuntime.DownGradePrice(30);
        */
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

        if (currentLevel == 1)
        {
            _UpgradeExpandArea.SetActive(true);

            for (int i = 0; i < _upgradeWhiteAreaPivots.Count; i++)
            {
                _whiteAreaPivots.Add(_upgradeWhiteAreaPivots[i]);
            }

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

        currentLevel++;
    }

    private string GetUpgradedFacilityID(string currentFacilityID)
    {
        if (string.IsNullOrWhiteSpace(currentFacilityID))
        {
            return string.Empty;
        }

        // 예:
        // FAC_RESTAURANT_01 -> FAC_RESTAURANT_02
        // FAC_ONSEN_01 -> FAC_ONSEN_02
        if (currentFacilityID.EndsWith("_01"))
        {
            return currentFacilityID.Substring(0, currentFacilityID.Length - 3) + "_02";
        }

        // 이미 고급 시설이면 그대로 반환
        if (currentFacilityID.EndsWith("_02"))
        {
            return currentFacilityID;
        }

        return string.Empty;
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
        _cameraController.ReturnToWorld();
        _canvas.gameObject.SetActive(false);

        if (_facilityRuntime != null)
        {
            UIManager.Instance.SetFurnitureButtonActive(_facilityRuntime.FacilityType, false);
        }

        UIManager.Instance._buildButton.SetActive(true);
        GridBuildingSystem.Instance.SetCurrentInBuilding(null);
    }

    private void InBuildingWhiteTilesCreate()
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

        if (currentLevel == 2)
        {
            _UpgradeExpandArea.SetActive(true);
        }

        /*
        _facilityRuntime.Gold = price;
        */

        Debug.Log("[InBuildingData] SetLevelPrice의 price 직접 대입 기능은 사용하지 않습니다. 가격은 SO 기준입니다.");
        InBuildingWhiteTilesCreate();
    }

    public void SetCurrentUseCount(int count)
    {
        _currentUseCount = count;
    }
}

/*
유니티 적용 방법
1. 기존 InBuildingData.cs를 이 코드로 교체합니다.
2. Gold / UpgradePrice / DownGradePrice 관련 테스트 코드는 모두 막아둔 상태입니다.
3. 이제 가격 관련 정보는 FacilityRuntime.UsageFee를 통해서만 확인하세요.
4. 이후 업그레이드는 가격 증가가 아니라, 상위 시설 ID로 교체하는 방식으로 연결하면 됩니다.
*/