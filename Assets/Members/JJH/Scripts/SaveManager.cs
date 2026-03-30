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
        data.MainTilemap = GridBuildingSystem.Instance.MainTilemap;
        data.BuildingList = GridBuildingSystem.Instance.BuildingList;
        data.PositionList = GridBuildingSystem.Instance.PositionList;
        data.OccupiedPositionList = GridBuildingSystem.Instance.OccupiedPositionList;
        data.TileTypes = GridBuildingSystem.Instance.TileTypes;
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);
    }

    public void Load()
    {
        if (!HasSave()) { Debug.LogWarning("세이브 파일 없음"); return; }

        string json = File.ReadAllText(_savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        for (int i = 0; i < data.OccupiedPositionList.Count; i++)
        {
            GridBuildingSystem.Instance.SetTileType(data.OccupiedPositionList[i], data.TileTypes[i]);
        }
    }
    
    public void DeleteSave()
    {
        if (File.Exists(_savePath))
            File.Delete(_savePath);
    }
}
