using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum FurnitureTouchType
{
    Restaurant,
    HotSpring,
    TrainingGround,
    Shop,
    VendingMachine
}

public enum ETileType
{
    Empty,
    White,
    Green,
    Red
}

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    public FacilityEffectDatabaseSO EffectDatabase;

    // grid 좌표에 Tile상태를 저장
    private Dictionary<Vector3Int, TileType> tileTypes = new Dictionary<Vector3Int, TileType>();

    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public Tilemap TempTilemap; // 프리뷰 표시용 타일맵

    private static Dictionary<ETileType, TileBase> _tileBases = new Dictionary<ETileType, TileBase>();

    public Building _temp; // 현재 배치 중인 건물( private -> public 바꿈)
    private Vector3 _prevPos; // 이전 마우스 셀 위치

    private HashSet<Vector3Int> occupied = new HashSet<Vector3Int>(); // 점유된 타일 좌표

    private bool _isPlacing = false; // 프리뷰 상태 체크
    [SerializeField] private BoundsInt _initialMapBounds;
    private InBuildingData _currentInBuildingData; // 내부건물 정보 저장용
    private int _saveframeCount = -1;

    //세이브용
    public List<Building> BuildingList = new List<Building>();
    public List<Vector3Int> OccupiedPositionList = new List<Vector3Int>();
    public List<TileType> TileTypes = new List<TileType>();

    [Header("클릭시 메뉴(재건축용)")]
    //가구 삭제용
    public GameObject RestaurantDelMenu;
    public GameObject HotSpringDelMenu;
    public GameObject TrainingGroundDelMenu;
    public GameObject ShopDelMenu;
    public GameObject VendingMachineDelMenu;
    //길 메뉴 셋팅용
    public GameObject RoadDelMenu;
    FurnitureTouchType furnitureTouchType;

    //#120 이슈 추가코드
    [Header("가구 설치 확인 버튼")]
    [SerializeField] private Button _restaurantPlaceButton;
    [SerializeField] private Button _hotSpringPlaceButton;
    [SerializeField] private Button _trainingGroundPlaceButton;
    [SerializeField] private Button _shopPlaceButton;
    [SerializeField] private Button _vendingMachinePlaceButton;
    [SerializeField] private Button _buildButton;

    // 길 설치용
    private HashSet<Vector3Int> _roadPathPositions = new HashSet<Vector3Int>();
    private bool _isDrawingRoad = false;
    private List<Building> _tempRoadObjects = new List<Building>();

    // 건물 재배치용
    private Vector3 _savedPosition;
    private BoundsInt _savedArea;
    private int _savedRotateCount;

    //===스마트폰 조작때 회전 제자리에 하는 불타입
    private bool _skipFollowOnce = false;
    
    // 시설, 가구 설치 비용
    public int GoldAmount { get; private set; }
    // 누적 수익
    public int UnlockRevenue { get; private set; }
    // 수용성 가구용 boolType
    public bool IsCanPlacing { get; private set; }
    // 최대 가구 배치 가능 개수
    public int MaxFurnitureCount { get; private set; }
    // 현재 가구 개수
    public int CurrentFurnitureCount { get; private set; }
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        furnitureTouchType = new FurnitureTouchType();
    }

    private void Start()
    {
        _tileBases.Add(ETileType.Empty, null);
        _tileBases.Add(ETileType.White, Resources.Load<TileBase>("SGH_Test/white"));
        _tileBases.Add(ETileType.Green, Resources.Load<TileBase>("SGH_Test/green"));
        _tileBases.Add(ETileType.Red, Resources.Load<TileBase>("SGH_Test/red"));
        if (_initialMapBounds.size == Vector3Int.zero)
            _initialMapBounds = MainTilemap.cellBounds;

        if (SaveManager.Instance.LoadMap)
        {
            SaveManager.Instance.Load();
            SaveManager.Instance.LoadMapChange();
            if (MapManager.Instance.MapLevel == 2) LevelUpCameraBounds();
        }
        else InitTileTypes();
    }

    private void Update()
    {
        if (_temp == null) return;

        if (_temp.buildType == BuildType.Road)
        {
            if (!_temp.Placed)
                HandleRoadTouch();
        }
        else if (!_temp.Placed)
        {
            HandleNormalBuildingTouch();
        }
    }

    // 길 전용 터치 처리
    private void HandleRoadTouch()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (IsTouchOverUI(touch)) return;

        Vector3Int cellPos = gridLayout.LocalToCell(Camera.main.ScreenToWorldPoint(touch.position));
        _temp.transform.position = gridLayout.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                _isDrawingRoad = true;
                ClearTempRoads();
                _roadPathPositions.Clear();

                bool canPlaceBegan = !occupied.Contains(cellPos) &&
                                     MainTilemap.GetTile(cellPos) == _tileBases[ETileType.White];

                if (canPlaceBegan && _roadPathPositions.Add(cellPos))
                    CreateRoadPreview(cellPos);
                break;

            case TouchPhase.Moved:
                if (_isDrawingRoad)
                {
                    bool canPlace = !occupied.Contains(cellPos) &&
                                    MainTilemap.GetTile(cellPos) == _tileBases[ETileType.White];

                    if (canPlace && _roadPathPositions.Add(cellPos))
                    {
                        CreateRoadPreview(cellPos);
                    }
                }

                break;

            case TouchPhase.Ended:
                _isDrawingRoad = false;
                break;
        }
    }

    // 기존 건물/가구 터치 처리
    private void HandleNormalBuildingTouch()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (IsTouchOverUI(touch)) return;

        if (touch.phase == TouchPhase.Ended)
        {
            Vector3Int cellPos = gridLayout.LocalToCell(Camera.main.ScreenToWorldPoint(touch.position));
            _temp.transform.localPosition = gridLayout.CellToLocalInterpolated(cellPos + new Vector3(0.5f, 0.5f, 0));
            _prevPos = cellPos;
            FollowBuilding();
        }
    }

    public void InitializeWithBuilding(GameObject building)
    {
        if (_isPlacing) return;

        Building buildingType = building.GetComponent<Building>();
        
        //선택한 버튼이 어떤 buildType인지에 따라 GoldAmount에 설치비용 저장
        //BuildType.Building이라면 필요 누적 수익도 UnlockRevenue에 저장
        switch (buildingType.buildType) 
        { 
            case BuildType.CapacityFurniture:
                GoldAmount = _currentInBuildingData.CapacityFurnitureData.interiorPrice;
                UnlockRevenue = 0;
                MaxFurnitureCount = _currentInBuildingData.MaxFurnitureCount;
                CurrentFurnitureCount = _currentInBuildingData.CurrentFurnitureCount;
                Debug.Log($"체크 스위치 문 : 시설 / 가구 비용 : {GoldAmount}");
                break;
            case BuildType.FeeFurniture:
                GoldAmount = _currentInBuildingData.FeeFurnitureData.interiorPrice;
                UnlockRevenue = 0;
                MaxFurnitureCount = _currentInBuildingData.MaxFurnitureCount;
                CurrentFurnitureCount = _currentInBuildingData.CurrentFurnitureCount;
                IsCanPlacing = true;
                Debug.Log($"체크 스위치 문 : 시설 / 가구 비용 : {GoldAmount}");
                break;
            case BuildType.Building:
                FacilityRuntime facilityRuntime = building.GetComponent<FacilityRuntime>();
                GoldAmount = EffectDatabase.GetBuildCostByFacilityID(facilityRuntime.FacilityID);
                UnlockRevenue = EffectDatabase.GetUnlockRevenueByFacilityID(facilityRuntime.FacilityID);
                IsCanPlacing = true;
                Debug.Log($"체크 스위치 문 : 시설 / 가구 비용 : {GoldAmount}");
                break;
            default:
                GoldAmount = 0;
                break;
        }
        if (GoldAmount > GoldTest.Instance._testGold) // 소지한 Gold가 설치비용보다 작다면 return
        {
            Debug.Log($"시설 / 가구 비용 : {GoldAmount}");
            Debug.Log("시설 / 가구 설치 불가");
            return;
        }

        if (UnlockRevenue > GoldTest.Instance.IncreasedGold) // 누적 수익이 설치비용보다 작다면 return
        {
            Debug.Log($"누적 수익 부족 / 현재 누적 수익{GoldTest.Instance.IncreasedGold} / 목표 누적 수익 {UnlockRevenue}");
            return;
        }

        int index = BuildingIndex(building);

        _temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        BuildingList.Add(_temp); // 세이브용
        MapManager.Instance.InstantiateInBuilding(_temp, index);
        _isPlacing = true;
        FollowBuilding();
    }

    public void InitializeBuilding(GameObject building) // 수용성 가구용
    {
        if (_isPlacing) return;

        Building buildingType = building.GetComponent<Building>();

        // BuildType이 수용성 가구고 개수가 3을 넘으면 버튼이 동작 안 함
        if (buildingType.buildType == BuildType.CapacityFurniture)
        {
            GoldAmount = _currentInBuildingData.CapacityFurnitureData.interiorPrice;
            MaxFurnitureCount = _currentInBuildingData.MaxFurnitureCount;
            CurrentFurnitureCount = _currentInBuildingData.CurrentFurnitureCount;
            UnlockRevenue = 0;
            Debug.Log($"가구 비용 : {GoldAmount}");
            if (GoldAmount > GoldTest.Instance._testGold) // 소지한 Gold가 설치비용보다 작다면 return
            {
                Debug.Log($"가구 설치 불가 / 가구 비용 {GoldAmount}");
                return;
            }
            
            // 수용성 가구가 MaxCapacityFurnitureCount 3개를 초과할 경우 return
            if (_currentInBuildingData.currentCapacityFurnitureCount >= _currentInBuildingData.MaxCapacityFurnitureCount)
            {
                IsCanPlacing = false;
                Debug.Log("더 이상 설치 불가!");
                return;
            }
            else
            {
                IsCanPlacing = true;
            }
        }

        int index = BuildingIndex(building);

        _temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        BuildingList.Add(_temp);
        MapManager.Instance.InstantiateInBuilding(_temp, index);
        _isPlacing = true;
        FollowBuilding();
    }

    // Green/Red 표시
    private void FollowBuilding()
    {
        TempTilemap.ClearAllTiles();
        _temp.area.position = gridLayout.WorldToCell(_temp.transform.position);
        BoundsInt area = _temp.area;

        int size = area.size.x * area.size.y;
        TileBase[] tiles = new TileBase[size];

        int i = 0;
        foreach (var pos in area.allPositionsWithin)
        {
            if (_temp.buildType == BuildType.TileBrush)
            {
                // TileBrush는 타일 없는 곳 = 초록, 이미 있는 곳 = 빨강
                tiles[i] = MainTilemap.GetTile(pos) == null
                    ? _tileBases[ETileType.Green]
                    : _tileBases[ETileType.Red];
            }
            else
            {
                tiles[i] = (occupied.Contains(pos) ||
                            MainTilemap.GetTile(pos) != _tileBases[ETileType.White])
                    ? _tileBases[ETileType.Red]
                    : _tileBases[ETileType.Green];
            }

            i++;

            //# 이슈 120
            TempTilemap.SetTilesBlock(area, tiles);
            bool canPlace = CanTakeArea(area);

            if (_temp.buildType == BuildType.CapacityFurniture || _temp.buildType == BuildType.FeeFurniture)
            {
                SetFurniturePlaceButtonInteractable(_temp.furnitureTouchType, canPlace);
            }
            else if(_temp.buildType == BuildType.Building)
            {
                SetBuildPlaceButton(canPlace);
            }
        }

        TempTilemap.SetTilesBlock(area, tiles);
    }

    // 설치 가능 여부 체크용 메서드
    public bool CanTakeArea(BoundsInt area)
    {
        if (_temp.buildType == BuildType.TileBrush)
        {
            foreach (var pos in area.allPositionsWithin)
            {
                if (MainTilemap.GetTile(pos) != null) return false;
            }

            return true;
        }

        foreach (var pos in area.allPositionsWithin)
        {
            if (occupied.Contains(pos)) return false;
            if (MainTilemap.GetTile(pos) != _tileBases[ETileType.White]) return false;
        }

        return true;
    }

    // 설치 관련 메서드
    public void TakeArea(BoundsInt area)
    
    {
        foreach (var pos in area.allPositionsWithin)
        {
            if (_temp.buildType == BuildType.TileBrush)
            {
                // occupied에 추가 안 하고 white 타일만 설치
                MainTilemap.SetTile(pos, _tileBases[ETileType.White]);
                SetTileType(pos, TileType.Tile);
            }
            else
            {
                occupied.Add(pos);
                if (_temp.buildType == BuildType.Road)
                    SetTileType(pos, TileType.Road);
                else if (_temp.buildType == BuildType.Building)
                    SetTileType(pos, TileType.Building);
                else if (_temp.buildType == BuildType.FeeFurniture) // 수익성 가구 오브젝트
                {
                    if (_saveframeCount != Time.frameCount)
                    {
                        if (SaveManager.Instance.LoadMap)
                        {
                            SetTileType(pos, TileType.FeeFurniture);
                            // _saveframeCount = Time.frameCount;
                            continue;
                        }
                        _currentInBuildingData.TryAssignProfitableFurniture();
                        SetTileType(pos, TileType.FeeFurniture);
                        _saveframeCount = Time.frameCount;
                    }
                }
                else if (_temp.buildType == BuildType.CapacityFurniture) // 수용성 가구 오브젝트
                {
                    if (_saveframeCount != Time.frameCount) //TakeArea가 한 프레임 내에 두번 호출돼서 한 번만 하게끔
                    {
                        if (SaveManager.Instance.LoadMap)
                        {
                            SetTileType(pos, TileType.CapacityFurniture);
                            continue;
                        }
                        
                        if (_currentInBuildingData.TryAssignCapacityFurniture())
                            SetTileType(pos, TileType.CapacityFurniture);

                        _saveframeCount = Time.frameCount;
                    }
                }
            }
        }

        TempTilemap.ClearAllTiles();
        MainTilemap.RefreshAllTiles();
    }

    // 오브젝트 재배치용 메서드
    public void ReleaseArea(BoundsInt area)
    {
        foreach (var pos in area.allPositionsWithin)
        {
            occupied.Remove(pos);
            OccupiedPositionList.Remove(pos); // 세이브용
            SetTileType(pos, TileType.Tile);
        }

        TempTilemap.ClearAllTiles();
        MainTilemap.RefreshAllTiles();
    }

    public bool IsOccupied(Vector3Int pos)
    {
        return occupied.Contains(pos);
    }

    // 해당 gird의 좌표에 무슨 TileType인지 알려줌
    public TileType GetTileType(Vector3Int pos)
    {
        if (tileTypes.TryGetValue(pos, out TileType type)) return type;

        return TileType.Empty;
    }

    // 해당 좌표에 어떤 TileType을 배치할지
    public void SetTileType(Vector3Int pos, TileType type) => tileTypes[pos] = type;

    // 맵 전체를 순회하면서 모든 타일 상태를 초기화하는 함수
    void InitTileTypes()
    {
        // 타일맵 전체 범위 가져오기
        BoundsInt bounds = _initialMapBounds;

        // 맵 안의 모든 좌표 하나씩 꺼냄
        foreach (var pos in bounds.allPositionsWithin)
        {
            MainTilemap.SetTile(pos, _tileBases[ETileType.White]);
            SetTileType(pos, TileType.Tile); // 전부 TileType.Empty(빈 상태)로 초기화
        }

        MainTilemap.RefreshAllTiles();
    }

    public int BuildingIndex(GameObject obj)
    {
        FacilityRuntime facilityRuntime = obj.GetComponentInChildren<FacilityRuntime>();

        if (facilityRuntime == null) return -1;

        return (int)facilityRuntime._facilityType - 1;
    }

    public void MapLevelUp()
    {
        if (MapManager.Instance.MapLevel != 1) return;

        // 현재 타일맵 범위 가져오기
        BoundsInt currentBounds = _initialMapBounds;

        int currentWidth = currentBounds.size.x;
        int currentHeight = currentBounds.size.y;

        // 우측으로 현재 너비만큼 확장
        BoundsInt rightArea = new BoundsInt(
            currentBounds.max.x,
            currentBounds.min.y,
            0,
            currentWidth,
            currentHeight * 2,
            1
        );

        // 상단으로 현재 높이만큼 확장
        BoundsInt topArea = new BoundsInt(
            currentBounds.min.x,
            currentBounds.max.y,
            0,
            currentWidth,
            currentHeight,
            1
        );

        TileBase whiteTile = _tileBases[ETileType.White];

        // 우측 영역 타일 생성
        foreach (var pos in rightArea.allPositionsWithin)
        {
            MainTilemap.SetTile(pos, whiteTile);
            SetTileType(pos, TileType.Tile);
        }

        // 상단 영역 타일 생성
        foreach (var pos in topArea.allPositionsWithin)
        {
            MainTilemap.SetTile(pos, whiteTile);
            SetTileType(pos, TileType.Tile);
        }

        MainTilemap.RefreshAllTiles();

        // 카메라 Bounds 업데이트
        LevelUpCameraBounds();
        MapManager.Instance.MapLevel++;
    }

    private void LevelUpCameraBounds()
    {
        CameraController cam = FindFirstObjectByType<CameraController>();
        if (cam == null) return;

        BoundsInt mapBounds = _initialMapBounds;

        cam.CameraBounds = new Bounds(
            mapBounds.center,
            cam.CameraBounds.size * 2f
        );

        cam.MaxSize *= 2f;
    }

    // enter 시 내부건물 참조용
    public void SetCurrentInBuilding(InBuildingData data)
    {
        _currentInBuildingData = data;
    }

    // ----------- 세이브 관련 -----------

    public void Save()
    {
        SaveManager.Instance.Save();
    }

    public void Load()
    {
        SaveManager.Instance.Load();
    }

    public void LoadSetTileType(Vector3Int pos, TileType type)
    {
        tileTypes[pos] = type;
        if (type != TileType.Empty)
        {
            MainTilemap.SetTile(pos, _tileBases[ETileType.White]);
            if (type == TileType.Road || type == TileType.Building)
            {
                if (!occupied.Contains(pos)) occupied.Add(pos);
            }
            else
            {
                if (occupied.Contains(pos)) occupied.Remove(pos);
            }
        }
        else
        {
            MainTilemap.SetTile(pos, null);
            if (occupied.Contains(pos)) occupied.Remove(pos);
        }
    }

    public void InitializeWithBuildingFromSave(GameObject prefab, BuildingSaveData bData)
    {
        _temp = Instantiate(prefab).GetComponent<Building>();

        _temp.transform.position = gridLayout.CellToWorld(bData.position) + new Vector3(0.5f, 0.5f, 0);
        for (int i = 0; i < bData.rotateCount; i++) _temp.Rotate();

        int index = BuildingIndex(prefab);
        MapManager.Instance.InstantiateInBuilding(_temp, index);

        if (_temp.InBuildingData != null)
        {
            _temp.InBuildingData.SetLevelPrice(bData.currentLevel, bData.BuildingGold);
            _temp.InBuildingData.SetCurrentCount(bData.CurrentUseCount, bData.FeeFurnitureCount, bData.CapacityFurnitureCount);
        }

        _temp.Place();

        BuildingList.Add(_temp);
        _temp = null;
    }

    //===============================================버튼 UI 연동
    // 회전
    public void RotateBuilding()
    {
        if (_temp == null) return;

        _temp.Rotate();
        _skipFollowOnce = true;
        FollowBuilding();
    }

    //설치
    public void PlaceCurrentBuilding()
    {
        if (_temp == null) return;

        if (_temp.buildType == BuildType.Road)
        {
            PlaceRoad();
            return;
        }

        if (!CanTakeArea(_temp.area))
        {
            UIManager.Instance.OnClickBuildTouchUICancel();
            return;
        }

        TakeArea(_temp.area);
        _temp.Place();
        PayFacilityGold(_temp);
        _temp = null;
        _isPlacing = false;
    }

    // 건물 철거
    public void DeleteSelectedBuilding()
    {
        if (_temp == null || !_temp.Placed) return;

        Building building = _temp;
        building.area.position = gridLayout.WorldToCell(building.transform.position);

        // occupied와 세이브 리스트 제거
        foreach (var pos in building.area.allPositionsWithin)
        {
            occupied.Remove(pos);
            OccupiedPositionList.Remove(pos); // 세이브용
            SetTileType(pos, TileType.Tile);
        }

        BuildingList.Remove(building);

        // 내부건물 데이터 연동 제거
        if (building.buildType == BuildType.CapacityFurniture)
            _currentInBuildingData.RemoveCapacityFurniture();
        else if (building.buildType == BuildType.FeeFurniture)
            _currentInBuildingData.RemoveProfitableFurniture();

        _temp.CloseMenu();
        GetFacilityRefundGold(building);
        building.DestroyBuilding();
        _temp = null;
        TempTilemap.ClearAllTiles();
        MainTilemap.RefreshAllTiles();
    }

    // 건물내에서 클릭한 건물에 메뉴 뜨게하는 코드. (프리펩으로 되어 있어 싱글톤인 그리드 시스템 이용)
    public void OnClickSetFurnitureMenu(GameObject buildingObj)
    {
        if (UIManager.Instance.OpenMenu == true) return;
        _temp = buildingObj.GetComponent<Building>(); // 클릭한 건물의 정보를 삭제를 위해 담음
        furnitureTouchType = _temp.furnitureTouchType;

        switch (furnitureTouchType)
        {
            case FurnitureTouchType.Restaurant:
                RestaurantDelMenu.SetActive(true);
                UIManager.Instance.OpenMenu = true;
                break;

            case FurnitureTouchType.HotSpring:
                HotSpringDelMenu.SetActive(true);
                UIManager.Instance.OpenMenu = true;
                break;

            case FurnitureTouchType.VendingMachine:
                VendingMachineDelMenu.SetActive(true);
                UIManager.Instance.OpenMenu = true;
                break;

            case FurnitureTouchType.TrainingGround:
                TrainingGroundDelMenu.SetActive(true);
                UIManager.Instance.OpenMenu = true;
                break;

            case FurnitureTouchType.Shop:
                ShopDelMenu.SetActive(true);
                UIManager.Instance.OpenMenu = true;
                break;
        }
    }

    public void OnClickSetRoadMenu(GameObject buildingObj)
    {
        if (UIManager.Instance.OpenMenu == true) return;
       
        _temp = buildingObj.GetComponent<Building>(); // 클릭한 건물의 정보를 삭제를 위해 담음
        RoadDelMenu.SetActive(true);
        UIManager.Instance.OpenMenu = true;
    }

    // 길 설치 프리뷰
    private void UpdateRoadPreview()
    {
        TempTilemap.ClearAllTiles();
        foreach (var pos in _roadPathPositions)
        {
            bool canPlace = !occupied.Contains(pos) &&
                            MainTilemap.GetTile(pos) == _tileBases[ETileType.White];

            TempTilemap.SetTile(pos, canPlace ? _tileBases[ETileType.Green] : _tileBases[ETileType.Red]);
        }
    }

    // 길 설치
// 길 확정 설치
    private void PlaceRoad()
    {
        foreach (var roadPiece in _tempRoadObjects)
        {
            Vector3Int pos = gridLayout.WorldToCell(roadPiece.transform.position);

            // 실제로 설치 가능한 칸(초록색)인 경우만 확정
            if (CanTakeArea(new BoundsInt(pos.x, pos.y, 0, 1, 1, 1)))
            {
                roadPiece.Place(); // Placed = true 및 관련 로직 실행
                BuildingList.Add(roadPiece);
                occupied.Add(pos);
                SetTileType(pos, TileType.Road);
            }
            else
            {
                // 설치 불가능한 위치(빨간색)에 생성된 건 삭제
                Destroy(roadPiece.gameObject);
            }
        }

        _tempRoadObjects.Clear();
        _roadPathPositions.Clear();
        TempTilemap.ClearAllTiles();

        // 원본 가이드 오브젝트 삭제
        if (_temp != null) Destroy(_temp.gameObject);
        _temp = null;
        _isPlacing = false;
    }

// CancelPlacement 메서드 상단에 ClearTempRoads() 추가
    public void CancelPlacement()
    {
        if (_temp == null) return;

        ClearTempRoads(); // 길 프리뷰 오브젝트들 삭제 추가

        TempTilemap.ClearAllTiles();
        if (!_temp.Placed) _temp.DestroyBuilding();

        _temp = null;
        _isPlacing = false;
    }

    // 두 점 사이 직선 좌표 반환 (x 또는 y 고정)
    private List<Vector3Int> GetLinePositions(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        if (start.x == end.x) // 세로
        {
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);
            for (int y = minY; y <= maxY; y++)
                result.Add(new Vector3Int(start.x, y, 0));
        }
        else // 가로
        {
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            for (int x = minX; x <= maxX; x++)
                result.Add(new Vector3Int(x, start.y, 0));
        }

        return result;
    }

    // 재배치 시작
    public void StartMoveCurrentBuilding()
    {
        if (_temp == null || !_temp.Placed) return;

        // 재배치 시작 전 현재 상태 저장
        _savedPosition = _temp.transform.position;
        _savedArea = _temp.area;
        _savedRotateCount = _temp.rotateCount;

        _temp.StartMove();
    }

    // 재배치 취소 (원위치 복원)
    public void CancelMoveBuilding()
    {
        if (_temp == null) return;

        TempTilemap.ClearAllTiles();

        // 회전 복원: 현재 rotateCount에서 저장된 rotateCount까지 역방향 회전
        int currentRotate = _temp.rotateCount;
        int targetRotate = _savedRotateCount;
        int diff = (currentRotate - targetRotate + 4) % 4;
        for (int i = 0; i < diff; i++) _temp.Rotate();

        // 위치 복원
        _temp.transform.position = _savedPosition;
        _temp.area = _savedArea;

        // 재점유
        Vector3Int positionInt = gridLayout.LocalToCell(_temp.transform.position);
        BoundsInt areaTemp = _temp.area;
        areaTemp.position = positionInt;

        TakeArea(areaTemp);
        _temp.RestorePlaced(); // Placed = true 복원
        OccupiedPositionList.Add(positionInt); // 세이브용

        _temp = null;
        _isPlacing = false;
    }


    //=== UI 위인지 아닌지 ===
    private bool IsTouchOverUI(Touch touch)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touch.position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            // UI Layer만 체크 (예: UI 레이어를 5로 설정했으면)
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }

        return false;
    }

    public void PayFacilityGold(Building building) // 건물 설치시 비용 지불 메서드
    {
        if (building.buildType == BuildType.Building)
        {
            int goldAmount = building.InBuildingData.FacilityRuntime.BuildCost;
            if (goldAmount > GoldTest.Instance._testGold)
            {
                Debug.Log("비용 부족 건물 설치 불가!");
                return;
            }

            GoldTest.Instance.PlayerUseMoney(goldAmount);
        }
    }

    public void GetFacilityRefundGold(Building building) // 건물 철거시 비용 환급 메서드
    {
        if (building.buildType == BuildType.Building)
        {
            int goldAmount = building.InBuildingData.FacilityRuntime.RefundAmount;
            GoldTest.Instance.GetFacilityRefundAmount(goldAmount);
        }
    }

    // public void PayFacilityGold(Building building)
    // {
    //     int goldAmount = 0;
    //     switch (building.buildType)
    //     {
    //         case BuildType.Building:
    //             goldAmount = building.InBuildingData.FacilityRuntime.BuildCost;
    //             break;
    //         case BuildType.CapacityFurniture:
    //             goldAmount = _currentInBuildingData.CapacityFurnitureData.interiorPrice;
    //             break;
    //         case BuildType.FeeFurniture:
    //             goldAmount = _currentInBuildingData.FeeFurnitureData.interiorPrice;
    //             break;
    //     }
    //     GoldTest.Instance.PlayerUseMoney(goldAmount);
    //     Debug.Log($"시설 설치 비용 : {goldAmount}");
    // }

    // 새로운 길 프리뷰 오브젝트 생성
    private void CreateRoadPreview(Vector3Int pos)
    {
        // 현재 들고 있는 길(_temp)을 복사해서 해당 위치에 배치
        Building roadPiece =
            Instantiate(_temp, gridLayout.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0), Quaternion.identity)
                .GetComponent<Building>();

        _tempRoadObjects.Add(roadPiece);

        UpdateRoadPreview();
    }

// 설치 취소 혹은 새로 시작할 때 임시 오브젝트들 삭제
    private void ClearTempRoads()
    {
        foreach (var obj in _tempRoadObjects)
        {
            if (obj != null) Destroy(obj.gameObject);
        }

        _tempRoadObjects.Clear();
        TempTilemap.ClearAllTiles();
    }

    //#120 이슈 관련 버튼 활성화 메서드
    private void SetFurniturePlaceButtonInteractable(FurnitureTouchType touchType, bool canPlace)
    {
        switch (touchType)
        {
            case FurnitureTouchType.Restaurant:
                if (_restaurantPlaceButton != null)
                    _restaurantPlaceButton.interactable = canPlace;
                break;

            case FurnitureTouchType.HotSpring:
                if (_hotSpringPlaceButton != null)
                    _hotSpringPlaceButton.interactable = canPlace;
                break;

            case FurnitureTouchType.TrainingGround:
                if (_trainingGroundPlaceButton != null)
                    _trainingGroundPlaceButton.interactable = canPlace;
                break;

            case FurnitureTouchType.Shop:
                if (_shopPlaceButton != null)
                    _shopPlaceButton.interactable = canPlace;
                break;

            case FurnitureTouchType.VendingMachine:
                if (_vendingMachinePlaceButton != null)
                    _vendingMachinePlaceButton.interactable = canPlace;
                break;        
        }
    }

    private void SetBuildPlaceButton(bool canPlace)
    {
        if (_buildButton != null)
            _buildButton.interactable = canPlace;
    }
}