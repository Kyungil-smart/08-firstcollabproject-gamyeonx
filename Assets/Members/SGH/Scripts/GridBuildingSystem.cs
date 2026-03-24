using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ETileType { Empty, White, Green, Red }

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    public GridLayout gridLayout;
    public Tilemap MainTilemap;   
    public Tilemap TempTilemap;     // 프리뷰 표시용 타일맵

    private static Dictionary<ETileType, TileBase> _tileBases = new Dictionary<ETileType, TileBase>();

    private Building _temp;     // 현재 배치 중인 건물     
    private Vector3 _prevPos;   // 이전 마우스 셀 위치

    private HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();   // 점유된 타일 좌표

    private bool _isPlacing = false;    // 프리뷰 상태 체크

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

        if (Input.GetMouseButtonDown(0) && !_isPlacing)
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

        if (_temp != null && Input.GetKeyDown(KeyCode.Space))
        {
            if (CanTakeArea(_temp.area))
            {
                TakeArea(_temp.area);
                _temp.Place();
                _temp = null;
                _isPlacing = false;
            }
        }
        else if (_temp != null && Input.GetKeyDown(KeyCode.Escape))
        {
            TempTilemap.ClearAllTiles();
            Destroy(_temp.gameObject);
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
                        occupied.Remove(pos);

                    MainTilemap.RefreshAllTiles();
                    Destroy(obj.gameObject);
                    break;
                }
            }
        }
    }

    public void InitializeWithBuilding(GameObject building)
    {
        if (_isPlacing) return;

        _temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
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
            if (occupied.Contains(pos) ||
                MainTilemap.GetTile(pos) != _tileBases[ETileType.White])
            {
                tiles[i] = _tileBases[ETileType.Red];
            }
            else
            {
                tiles[i] = _tileBases[ETileType.Green];
            }
            i++;
        }

        TempTilemap.SetTilesBlock(area, tiles);
    }

    // 설치 가능 여부 체크용 메서드
    public bool CanTakeArea(BoundsInt area)
    {
        foreach (var pos in area.allPositionsWithin)
        {
            if (occupied.Contains(pos))
                return false;

            if (MainTilemap.GetTile(pos) != _tileBases[ETileType.White])
                return false;
        }
        return true;
    }

    // 설치 관련 메서드
    public void TakeArea(BoundsInt area)
    {
        foreach (var pos in area.allPositionsWithin)
            occupied.Add(pos);

        TempTilemap.ClearAllTiles();
    }

    public void ReleaseArea(BoundsInt area)
    {
        foreach (var pos in area.allPositionsWithin)
            occupied.Remove(pos);

        TempTilemap.ClearAllTiles();
        MainTilemap.RefreshAllTiles();
    }
}