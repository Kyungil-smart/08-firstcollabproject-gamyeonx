using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    private Building _temp; // 현재 배치 중인 건물     
    private Vector3 _prevPos; // 이전 마우스 셀 위치

    private HashSet<Vector3Int> occupied = new HashSet<Vector3Int>(); // 점유된 타일 좌표

    private bool _isPlacing = false; // 프리뷰 상태 체크

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
        InitTileTypes();
    }

    private void Update()
    {
        if (_temp != null && !_temp.Placed)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = gridLayout.LocalToCell(mousePos);

            if (_prevPos != cellPos)
            {
                _temp.transform.localPosition =
                    gridLayout.CellToLocalInterpolated(cellPos + new Vector3(0.5f, 0.5f, 0f));
                _prevPos = cellPos;
                FollowBuilding();
            }
        }

        if (Input.GetMouseButtonDown(2) && !_isPlacing)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = gridLayout.LocalToCell(mousePos);

            foreach (var obj in FindObjectsOfType<Building>())
            {
                if (!obj.Placed) continue;
                if (obj.area.Contains(cellPos))
                {
                    _temp = obj;
                    _temp.StartMove();
                    _isPlacing = true;
                    break;
                }
            }
        }

        if (_temp != null)
        {
            bool shouldPlace = (_temp.buildType == BuildType.TileBrush || _temp.buildType == BuildType.Road)
                ? Input.GetMouseButton(0)
                : Input.GetMouseButtonDown(0);

            if (shouldPlace && CanTakeArea(_temp.area))
            {
                TakeArea(_temp.area);

                if (_temp.buildType == BuildType.TileBrush)
                {
                    FollowBuilding();
                }
                else if (_temp.buildType == BuildType.Road)
                {
                    GameObject roadPrefab = _temp.gameObject;
                    _temp.Place();

                    // 현재 마우스 위치 계산
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3Int cellPos = gridLayout.LocalToCell(mousePos);
                    Vector3 spawnPos = gridLayout.CellToLocalInterpolated(cellPos + new Vector3(0.5f, 0.5f, 0f));

                    // 마우스 위치에 바로 생성
                    _temp = Instantiate(roadPrefab, spawnPos, Quaternion.identity).GetComponent<Building>();
                    _isPlacing = true;
                    _prevPos = Vector3.zero;
                    FollowBuilding();
                }
                else
                {
                    _temp.Place();
                    _temp = null;
                    _isPlacing = false;
                }
            }
        }

        if (_temp != null && Input.GetKeyDown(KeyCode.Escape))
        {
            TempTilemap.ClearAllTiles();
            _temp.DestroyBuilding();
            _isPlacing = false;
        }

        if (Input.GetMouseButtonDown(1) && !_isPlacing)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = gridLayout.LocalToCell(mousePos);

            foreach (var obj in FindObjectsOfType<Building>())
            {
                if (!obj.Placed) continue;
                if (obj.area.Contains(cellPos))
                {
                    foreach (var pos in obj.area.allPositionsWithin)
                    {
                        occupied.Remove(pos);
                        // 연동준이 고침  
                        SetTileType(pos, TileType.Empty);
                    }

                    MainTilemap.RefreshAllTiles();
                    obj.DestroyBuilding();
                    break;
                }
            }
        }

        if (_temp != null && Input.GetKeyDown(KeyCode.G))
        {
            _temp.Rotate();
            FollowBuilding();
        }
    }

    public void InitializeWithBuilding(GameObject building)
    {
        if (_isPlacing) return;

        int index = BuildingIndex(building);

        _temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
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
                SetTileType(pos, TileType.Empty);
            }
            else
            {
                occupied.Add(pos);
                if (_temp.buildType == BuildType.Road)
                    SetTileType(pos, TileType.Road);
                else if (_temp.buildType == BuildType.Building)
                    SetTileType(pos, TileType.Building);
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
            SetTileType(pos, TileType.Empty);
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
        BoundsInt bounds = MainTilemap.cellBounds;

        // 맵 안의 모든 좌표 하나씩 꺼냄
        foreach (var pos in bounds.allPositionsWithin)
            SetTileType(pos, TileType.Empty); // 전부 TileType.Empty(빈 상태)로 초기화
    }

    public int BuildingIndex(GameObject obj)
    {
        FacilityRuntime facilityRuntime = obj.GetComponentInChildren<FacilityRuntime>();

        if (facilityRuntime == null) return -1;

        return (int)facilityRuntime._facilityType - 1;
    }
}