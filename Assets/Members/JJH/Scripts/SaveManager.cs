using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string _savePath;
    public bool LoadMap = false;
    public SaveData data = new SaveData();
    
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
        if (GridBuildingSystem.Instance == null || MapManager.Instance == null) 
        {
            Debug.LogError("Manager Instance가 없습니다!");
            return;
        }
        if (GridBuildingSystem.Instance.MainTilemap == null)
        {
            Debug.LogError("MainTilemap을 찾을 수 없습니다!");
            return;
        }
        
        data.Buildings.Clear(); 
        data.OccupiedPositionList.Clear();
        data.TileTypes.Clear();
        
        data.MapLevel = MapManager.Instance.MapLevel;
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance._gameTime != null)
            {
                data.UserWeek = UIManager.Instance._gameTime._userWeek;
            }

            if (UIManager.Instance._goldTest != null)
            {
                data.Gold = UIManager.Instance._goldTest.TestGoldValue;
                data.IncreasedGold = UIManager.Instance._goldTest.IncreasedGold;
            }
                
            
            data.TrigeredEvents = new List<string>(UIManager.Instance._triggeredEvents);
        }
        // 건물 정보 저장
        foreach (Building b in GridBuildingSystem.Instance.BuildingList)
        {
            if (b == null || b.gameObject == null || !b.Placed) continue;
            
            int level = 1;
            int useCount = 4;
            int gold = 100;
            int furnitureCount = 0;
            int capacityFurnitureCount = 0;
            
            if (b.InBuildingData != null)
            {
                level = b.InBuildingData.currentLevel;
                useCount = b.InBuildingData._currentUseCount;
                gold = b.InBuildingData.FacilityRuntime.FurnitureGold;
                furnitureCount = b.InBuildingData._currentFeeFurnitureCount;
                capacityFurnitureCount = b.InBuildingData._currentCapacityFurnitureCount;
            }
            
            BuildingSaveData bData = new BuildingSaveData {
                prefabName = b.name.Replace("(Clone)", "").Trim(), // 프리펩 이름 저장
                position = GridBuildingSystem.Instance.gridLayout.WorldToCell(b.transform.position),
                rotateCount = b.rotateCount,
                currentLevel = level,
                CurrentUseCount = useCount,
                BuildingGold = gold,
                FeeFurnitureCount = furnitureCount,
                CapacityFurnitureCount = capacityFurnitureCount
                // currentStat = b.InBuildingData.stat; // 필요시 스탯 추가
            };
            data.Buildings.Add(bData);
        }
        // 타일맵 정보 저장
        BoundsInt bounds = GridBuildingSystem.Instance.MainTilemap.cellBounds;
        
        foreach (var pos in bounds.allPositionsWithin)
        {
            TileType type = GridBuildingSystem.Instance.GetTileType(pos);
            if (data.OccupiedPositionList == null) data.OccupiedPositionList = new List<Vector3IntSaveData>();
            data.OccupiedPositionList.Add(new Vector3IntSaveData(pos));
            data.TileTypes.Add(type);
        }
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);
    }

    public void Load()
    {
        if (!HasSave()) { Debug.LogWarning("세이브 파일 없음"); return; }

        string json = File.ReadAllText(_savePath);
        data = JsonUtility.FromJson<SaveData>(json);
        
        GridBuildingSystem.Instance.MainTilemap.ClearAllTiles();
        MapManager.Instance.MapLevel = data.MapLevel;
        UIManager.Instance._gameTime.UserWeek = data.UserWeek;
        UIManager.Instance._goldTest.TestGoldValue = data.Gold;
        UIManager.Instance._goldTest.IncreasedGold = data.IncreasedGold;
        UIManager.Instance._triggeredEvents = new HashSet<string>(data.TrigeredEvents);
        // 타일 생성
        for (int i = 0; i < data.OccupiedPositionList.Count; i++)
        {
            Vector3Int pos = data.OccupiedPositionList[i].SaveData();
            GridBuildingSystem.Instance.LoadSetTileType(pos, data.TileTypes[i]);
        }
        // 건물 생성
        foreach (var bData in data.Buildings)
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Buildings/{bData.prefabName}");
            GridBuildingSystem.Instance.InitializeWithBuildingFromSave(prefab, bData);
        }

        // EventManager.Instance.LoadTriggerEvents(); // 이벤트 불러오기
        
        GridBuildingSystem.Instance.MainTilemap.RefreshAllTiles();
    }
    
    public void DeleteSave()
    {
        if (File.Exists(_savePath))
            File.Delete(_savePath);
    }

    public void LoadButton()
    {
        if (!HasSave()) { Debug.LogWarning("세이브 파일 없음"); return; }
        LoadMapChange();
        SceneManager.LoadScene(1);
        AudioManager.Instance.PlaySceneBGM("MapScene");
    }

    public void LoadMapChange()
    {
        LoadMap = !LoadMap;
    }
    
    public void StartNewGame() {
        LoadMap = false;
        data = new SaveData();
        SceneManager.LoadScene(1);
    }
}
