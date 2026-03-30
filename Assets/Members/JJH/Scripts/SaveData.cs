using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class SaveData
{
    public Tilemap MainTilemap;
    public List<Building> BuildingList;
    public List<Position> PositionList;
    public List<Vector3Int> OccupiedPositionList;
    public List<TileType> TileTypes;
}
