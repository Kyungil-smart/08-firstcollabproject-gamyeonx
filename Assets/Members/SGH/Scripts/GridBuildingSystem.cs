using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public enum ETileType { Empty, White, Green, Red }

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }
    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public Tilemap TempTilemap;

    private static Dictionary<ETileType, TileBase> _tileBases = new Dictionary<ETileType, TileBase>();

    private Building _temp;
    private Vector3 _prevPos;
    private BoundsInt prevArea;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
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
        if (!_temp) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject(0))
            {
                return;
            }

            if (!_temp.Placed)
            {
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = gridLayout.LocalToCell(touchPos);

                if (_prevPos != cellPos)
                {
                    _temp.transform.localPosition = gridLayout.CellToLocalInterpolated(cellPos
                        + new Vector3(0.5f, 0.5f, 0f));
                    _prevPos = cellPos;
                    FollowBuilding();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_temp.CanbePlaced())
            {
                _temp.Place();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearArea();
            Destroy(_temp.gameObject);
        }
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    private static void SetTilesBlock(BoundsInt area, ETileType type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }

    private static void FillTiles(TileBase[] arr, ETileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = _tileBases[type];
        }
    }

    public void InitializeWithBuilding(GameObject building)
    {
        _temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        FollowBuilding();
    }

    private void ClearArea()
    {
        TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(toClear, ETileType.Empty);
        TempTilemap.SetTilesBlock(prevArea, toClear);
    }

    private void FollowBuilding()
    {
        ClearArea();

        _temp.area.position = gridLayout.WorldToCell(_temp.gameObject.transform.position);
        BoundsInt buildingArea = _temp.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, MainTilemap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];

        for (int i = 0; i < baseArray.Length; i++)
        {
            if (baseArray[i] == _tileBases[ETileType.White])
            {
                tileArray[i] = _tileBases[ETileType.Green];
            }
            else
            {
                FillTiles(tileArray, ETileType.Red);
                break;
            }
        }

        TempTilemap.SetTilesBlock(buildingArea, tileArray);
        prevArea = buildingArea;
    }

    public bool CanTakeArea(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);
        foreach (var b in baseArray)
        {
            if (b != _tileBases[ETileType.White])
            {
                Debug.Log("Cannot place here");
                return false;
            }
        }

        return true;
    }

    public void TakeArea(BoundsInt area)
    {
        SetTilesBlock(area, ETileType.Empty, TempTilemap);
        SetTilesBlock(area, ETileType.Green, MainTilemap);

    }
}