using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

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

    //가구 삭제용
    public GameObject FurnitureMenu;

    //===스마트폰 조작때 회전 제자리에 하는 불타입
    private bool _skipFollowOnce = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
        if (_temp != null && !_temp.Placed && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // UI 위면 무시
            if (IsTouchOverUI(touch))
                return;

            if (touch.phase == TouchPhase.Ended)
            {
                if (_skipFollowOnce)
                {
                    _skipFollowOnce = false;
                    return;
                }

                Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                Vector3Int cellPos = gridLayout.LocalToCell(touchPos);

                _temp.transform.localPosition =
                    gridLayout.CellToLocalInterpolated(cellPos + new Vector3(0.5f, 0.5f, 0f));

                _prevPos = cellPos;
                FollowBuilding();
            }
        }
    }

    public void InitializeWithBuilding(GameObject building)
    {
        if (_isPlacing) return;

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
            if (_currentInBuildingData.currentCapacityFurnitureCount >= 3)
            {
                Debug.Log("더 이상 설치 불가!");
                return;
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
                        SetTileType(pos, TileType.FeeFurniture);
                        _currentInBuildingData.TryAssignProfitableFurniture();
                        _saveframeCount = Time.frameCount;
                    }
                }
                else if (_temp.buildType == BuildType.CapacityFurniture) // 수용성 가구 오브젝트
                {
                    if (_saveframeCount != Time.frameCount) //TakeArea가 한 프레임 내에 두번 호출돼서 한 번만 하게끔
                    {
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
            _temp.InBuildingData.SetCurrentUseCount(bData.CurrentUseCount);
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
    // 건설중 취소
    public void CancelPlacement()
    {
        if (_temp == null) return;

        TempTilemap.ClearAllTiles();

        if (!_temp.Placed)
        {
            _temp.DestroyBuilding();
        }

        _temp = null;
        _isPlacing = false;
    }
    //설치
    public void PlaceCurrentBuilding()
    {
        if (_temp == null) return;
        if (!CanTakeArea(_temp.area)) return;

        TakeArea(_temp.area);
        _temp.Place();
        _temp = null;
        _isPlacing = false;
    }
    // 건물 철거
    public void DeleteSelectedBuilding()
    {
        if (_temp == null || !_temp.Placed) return;

        Building building = _temp;

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

        building.DestroyBuilding();
        _temp = null;
        TempTilemap.ClearAllTiles();

    }

    // 건물내에서 클릭한 건물에 메뉴 뜨게하는 코드. (프리펩으로 되어 있어 싱글톤인 그리드 시스템 이용)
    public void OnClickSetFurnitureMenu(GameObject buildingObj)
    {
        _temp = buildingObj.GetComponent<Building>(); // 클릭한 건물의 정보를 삭제를 위해 담음
        FurnitureMenu.SetActive(true);
    }

    //=== UI 위인지 아닌지
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
}
