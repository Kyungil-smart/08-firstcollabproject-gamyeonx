using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string _savePath;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }
    
    public bool HasSave()
    {
        return File.Exists(_savePath);
    }

    public void Save()
    {
        SaveData data = new SaveData();
        BoundsInt bounds = GridBuildingSystem.Instance.MainTilemap.cellBounds;
        
        foreach (var pos in bounds.allPositionsWithin)
        {
            TileType type = GridBuildingSystem.Instance.GetTileType(pos);
            data.OccupiedPositionList.Add(new Vector3IntSaveData(pos));
            data.TileTypes.Add(type);
        }
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);
    }

    public void Load()
    {
        if (!HasSave()) { Debug.LogWarning("세이브 파일 없음"); return; }
        
        GridBuildingSystem.Instance.MainTilemap.ClearAllTiles();

        string json = File.ReadAllText(_savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        for (int i = 0; i < data.OccupiedPositionList.Count; i++)
        {
            Vector3Int pos = data.OccupiedPositionList[i].SaveData();
            GridBuildingSystem.Instance.LoadSetTileType(pos, data.TileTypes[i]);
        }
        
        GridBuildingSystem.Instance.MainTilemap.RefreshAllTiles();
    }
    
    public void DeleteSave()
    {
        if (File.Exists(_savePath))
            File.Delete(_savePath);
    }
}
