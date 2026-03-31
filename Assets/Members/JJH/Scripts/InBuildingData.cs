using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System;

public class InBuildingData : MonoBehaviour
{
    [Header("카메라 기준점")]
    public GameObject CameraPivot;
    
    [Header("캔버스 및 버튼")]   
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _returnButton;

    [Header("건물 정보")]
    public int currentLevel { get; private set; } = 1;
    public int maxLevel { get; private set; } = 2;
    
    [Header("다음레벨 프리팹")]
    [SerializeField] private List<GameObject> _nextLevelPrefabs;
    [SerializeField] private GameObject _UpgradeExpandArea; 
    
    private FacilityRuntime _facilityRuntime;
    public FacilityRuntime FacilityRuntime => _facilityRuntime;
    
    [Header("입구/웨이팅/사용 정보")]
    public GameObject EnterPivot;
    public GameObject WaitPivot;
    public GameObject UsePivot;
    public List<GameObject> UsePivots; // 시설 이용 공간(현재 기준 7개)
    public GameObject FacilityExitPivot; // 시설 외부로 나가는 출구
    
    public GameObject EntrancePivot; // 시설 이용 입구
    public GameObject ExitPivot; // 시설 이용 출구
    public List<GameObject> EntranceWayPivots; // 시설 이용으로 가는 길
    public List<GameObject> ExitWayPivots; // 시설 이용 후 나가는 길
    
    [Header("내부 그리드 설정")]
    public List<GameObject> _whiteAreaPivots;  // 흰 타일 생성 위치 목록
    public List<GameObject> _upgradeWhiteAreaPivots; // 업그레이드 후 휜 타일 생성 위치 목록
    private Vector2Int _whiteAreaSize;  // 각 피벗에서 생성할 크기
    
    CameraController _cameraController;
    
    [Header("시설 이용 공간 설정")]
    [SerializeField] private int _defaultUseCount = 4; // 시설 이용 공간 기본 개수
    [SerializeField] private int _currentUseCount; // 현재 시설 이용 가능 공간 개수
    private List<Transform> _usePivotsTransformsList = new List<Transform>(); // 시설 이용 가능한 공간들에 대한 Transform 값들을 넣어놓은 List
    public event Action<List<Transform>> OnUsePivotsChanged; // 시설 이용 가능 공간이 바뀔 때마다 호출될 델리게이트

    private void Awake()
    {
        _canvas.gameObject.SetActive(false);
        _whiteAreaSize =  new Vector2Int(1, 1);
        _UpgradeExpandArea.SetActive(false);

        _currentUseCount = _defaultUseCount; // 현재 시설 이용 가능 공간을 기본 공간 개수로 초기화
    }

    private void Start()
    {
        _returnButton.onClick.AddListener(ReturnButton);
        _cameraController = FindFirstObjectByType<CameraController>();
        InBuildingWhiteTilesCreate();  // 내부 진입 시 흰 타일 자동 생성
        
        UpdateUseVisual();    
        OnUsePivotsChanged?.Invoke(GetUsePivots());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            IncreaseUsePivots();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            DecreaseUsePivots();
        }
    }

    // 현재 늘어난 이용 공간을 시각적으로 보여주는 메서드
    public void UpdateUseVisual() 
    {
        // 기본으로 제공하는 사용 공간은 이미 보여주고 있고, 더 줄어들 일도 없으므로 _defaultUseCount부터 시작
        for (int i = _defaultUseCount; i < UsePivots.Count; i++)
        {
            bool isActive = i < _currentUseCount;
            UsePivots[i].SetActive(isActive);
        }
    }

    // 시설 이용 공간이 늘어날 때 사용하는 메서드
    public void IncreaseUsePivots()
    {
        if (_currentUseCount >= UsePivots.Count) return;
        _currentUseCount++;
        UpdateUseVisual();

        OnUsePivotsChanged?.Invoke(GetUsePivots());
    }

    // 시설 이용 공간이 줄어들 때 사용하는 메서드
    public void DecreaseUsePivots()
    {
        if (_currentUseCount <= _defaultUseCount) return;
        _currentUseCount--;
        UpdateUseVisual();

        OnUsePivotsChanged?.Invoke(GetUsePivots());
    }
    
    
    // 현재 시설 이용 가능 공간들의 Transform 값들을 반환해주는 메서드
    public List<Transform> GetUsePivots()
    {
        _usePivotsTransformsList.Clear();
        
        for (int i = 0; i < _currentUseCount; i++)
        {
            _usePivotsTransformsList.Add(UsePivots[i].transform);
        }
    
        return _usePivotsTransformsList;
    }
    

    public void BuildingEntered()
    {
        _canvas.gameObject.SetActive(true);
        GridBuildingSystem.Instance.SetCurrentInBuilding(this);
    }

    public void SetFacilityRuntime(FacilityRuntime facilityRuntime)
    {
        _facilityRuntime = facilityRuntime;
    }

    public void BuildingLevelUp()
    {
        if (maxLevel < currentLevel) return;
        // float X = CameraPivot.transform.position.x;
        // float Y = CameraPivot.transform.position.y;
        // Vector2 NextLevelPivot = new Vector2(X, Y);
        // GameObject nextLevelInstance = Instantiate(_nextLevelPrefabs[currentLevel - 1], NextLevelPivot, CameraPivot.transform.rotation);
        // LevelUpBuildingData levelUpData = nextLevelInstance.GetComponent<LevelUpBuildingData>();
        // if (levelUpData != null)
        // {
        //     // 3. 한 프레임 쉬고 실행하거나, 강제로 좌표 동기화
        //     Physics2D.SyncTransforms(); 
        //     levelUpData.LevelUpBuildingWhiteTilesCreate();
        // }
        if (currentLevel == 1)
        {
            _UpgradeExpandArea.SetActive(true); // 업그레이드 프리팹 SetActive(true)
            for (int i = 0; i < _upgradeWhiteAreaPivots.Count; i++)
            {
                _whiteAreaPivots.Add(_upgradeWhiteAreaPivots[i]); // 업그레이드 프리팹에 휜 타일 생성 위치 갱신
            }

            InBuildingWhiteTilesCreate(); // 업그레이드 프리팹에 휜 타일 생성
        }
        else if (currentLevel == 2) _facilityRuntime.UpgradePrice(20);
        Debug.Log($"<color=yellow/>현재 가격 {_facilityRuntime.Gold}</color>");
        currentLevel++;
    }

    public void ReturnButton()
    {
        _cameraController.ReturnToWorld();
        _canvas.gameObject.SetActive(false);
        UIManager.Instance.SetFurnitureButtonActive(_facilityRuntime.FacilityType, false);
        UIManager.Instance._buildButton.SetActive(true);
        GridBuildingSystem.Instance.SetCurrentInBuilding(null);
    }
    
    private void InBuildingWhiteTilesCreate()
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
                    TileBase currentTile = GridBuildingSystem.Instance.MainTilemap.GetTile(pos);
                    if (currentTile == whiteTile) continue; // 이미 휜 타일이 있다면 무시
                    
                    GridBuildingSystem.Instance.MainTilemap.SetTile(pos, whiteTile);
                }
            }
        }
    }
    
    // 세이브 로드 관련
    public void SetLevel(int level)
    {
        currentLevel = level;
        if(currentLevel >= 2) _UpgradeExpandArea.SetActive(true);
        InBuildingWhiteTilesCreate(); 
    }
}
