using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

[System.Serializable]
public class Vector3IntSaveData
{
    public int x, y, z;

    public Vector3IntSaveData(Vector3Int pos)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }
    
    public Vector3Int SaveData() => new Vector3Int(x, y, z);
}


[System.Serializable]
public class SaveData
{
    public List<Building> BuildingList;
    public List<Position> PositionList;
    public List<Vector3IntSaveData> OccupiedPositionList = new List<Vector3IntSaveData>();
    public List<TileType> TileTypes = new List<TileType>();
}
