using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
    
    [Header("입구/웨이팅/사용 정보")]
    public GameObject EnterPivot;
    public GameObject WaitPivot;
    public GameObject UsePivot;
    public List<GameObject> UsePivots;

    public GameObject EntrancePivot;
    public GameObject ExitPivot;
    public List<GameObject> EntranceWayPivots;
    public List<GameObject> ExitWayPivots;
    
    
    [Header("내부 그리드 설정")]
    public List<GameObject> _whiteAreaPivots;  // 흰 타일 생성 위치 목록
    public List<GameObject> _upgradeWhiteAreaPivots; // 업그레이드 후 휜 타일 생성 위치 목록
    private Vector2Int _whiteAreaSize;  // 각 피벗에서 생성할 크기
    
    CameraController _cameraController;

    private void Awake()
    {
        _canvas.gameObject.SetActive(false);
        _whiteAreaSize =  new Vector2Int(1, 1);
        _UpgradeExpandArea.SetActive(false);
    }

    private void Start()
    {
        _returnButton.onClick.AddListener(ReturnButton);
        _cameraController = FindFirstObjectByType<CameraController>();
        InBuildingWhiteTilesCreate();  // 내부 진입 시 흰 타일 자동 생성
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
                _whiteAreaPivots.Add(_upgradeWhiteAreaPivots[i]); // 업그레이드 프리팹에 있는 휜 타일 생성 위치 갱신
            }

            InBuildingWhiteTilesCreate(); // 업그레이드 프리팹에 있는 휜 타일 생성
        }
        else if (currentLevel == 2) _facilityRuntime.UpgradePrice(20);
        Debug.Log($"<color=yellow/>현재 가격 {_facilityRuntime.Gold}</color>");
        currentLevel++;
    }

    public void ReturnButton()
    {
        _cameraController.ReturnToWorld();
        _canvas.gameObject.SetActive(false);
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
