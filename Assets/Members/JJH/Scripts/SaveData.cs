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
public class BuildingSaveData
{
    public string prefabName;
    public Vector3Int position;
    public int rotateCount;
    public int currentLevel; // 내부 건물 레벨
    // public float currentStat; // 필요시 스탯 추가
}


[System.Serializable]
public class SaveData
{
    public List<BuildingSaveData> Buildings = new List<BuildingSaveData>();
    public List<Vector3IntSaveData> OccupiedPositionList = new List<Vector3IntSaveData>();
    public List<TileType> TileTypes = new List<TileType>();
    public int MapLevel;
}
