using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelUpBuildingData : MonoBehaviour
{
    public List<GameObject> _whiteAreaPivots;
    private Vector2Int _whiteAreaSize = new Vector2Int(1, 1);
    
    public void LevelUpBuildingWhiteTilesCreate()
    {
        if (GridBuildingSystem.Instance.MainTilemap == null) return;

        TileBase whiteTile = Resources.Load<TileBase>("SGH_Test/white");

        foreach (var pivot in _whiteAreaPivots)
        {
            Vector3Int cellPos = GridBuildingSystem.Instance.MainTilemap.WorldToCell(pivot.transform.position);
            
            for (int x = 0; x < _whiteAreaSize.x; x++)
            {
                for (int y = 0; y < _whiteAreaSize.y; y++)
                {
                    Vector3Int pos = new Vector3Int(cellPos.x + x, cellPos.y + y, 0);
                    GridBuildingSystem.Instance.MainTilemap.SetTile(pos, whiteTile);
                }
            }
        }
    }
}
