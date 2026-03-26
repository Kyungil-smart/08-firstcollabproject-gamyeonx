using UnityEngine;
using UnityEngine.UI;

public enum BuildType
{
    Road,
    Building
}

public class Building : MonoBehaviour
{
    public bool Placed { get; private set; }    // 설치 완료 여부
    public BoundsInt area;  // 건물이 차지하는 영역 오프셋
    public BuildType buildType;
    
    private Canvas _canvas;
    private Button _enterButton;
    private Button _levelUpButton;
    CameraController _cameraController;
    
    public InBuildingData InBuildingData;
    public GameObject InBuildingRoot;

    
    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>(true);
        _enterButton = _canvas?.transform.Find("Enter")?.GetComponent<Button>();
        _levelUpButton = _canvas?.transform.Find("LevelUp")?.GetComponent<Button>();
        _canvas?.gameObject.SetActive(false);
    }
    
    private void Start()
    {
        _cameraController = FindFirstObjectByType<CameraController>();
        _enterButton?.onClick.AddListener(BuildingEntered);
        _levelUpButton?.onClick.AddListener(BuildingLevelUp);
    }

    public void CanvasActive()
    {
        _cameraController.SetInputLock(true);
        _canvas?.gameObject.SetActive(true);
    }

    public void BuildingEntered()
    {
        _cameraController.MoveToBuilding(
            pivot      : InBuildingData.CameraPivot.transform,
            boundsSize : new Vector2(InBuildingData.currentLevel * 10, InBuildingData.currentLevel * 10), 
            minSize    : InBuildingData.currentLevel * 2f,
            maxSize    : InBuildingData.currentLevel * 6f
        );
        
        _canvas?.gameObject.SetActive(false);
        InBuildingData.BuildingEntered();
        // _cameraController.SetInputLock(false);
    }

    public void BuildingLevelUp()
    {
        // 재화 있고 최대레벨 이하면 내부 건물 스크립트의 Levelup 불러오기
        // if (현재재화 < 필요재화) return;
        InBuildingData.BuildingLevelUp();
        _canvas?.gameObject.SetActive(false);
        _cameraController.SetInputLock(false);
    }
    
    // 현재 위치에서 설치 가능여부 체크용 메서드
    public bool CanbePlaced()
    {
        Vector3Int positionInt = GridBuildingSystem.Instance.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;

        if (GridBuildingSystem.Instance.CanTakeArea(areaTemp))
        {
            return true;
        }

        return false;
    }

    // 설치 처리용 메서드
    public void Place()
    {
        Vector3Int positionInt = GridBuildingSystem.Instance.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        Placed = true;
        GridBuildingSystem.Instance.TakeArea(areaTemp);
    }

    // 오브젝트 회전시키는 메서드
    public void Rotate()
    {
        transform.Rotate(0, 0, -90);

        var size = area.size;
        area.size = new Vector3Int(size.y, size.x, size.z);
    }

    // 오브젝트 재배치용 메서드
    public void StartMove()
    {
        if (!Placed) return;
        GridBuildingSystem.Instance.ReleaseArea(area);
        Placed = false;
    }
    
    // 건물 삭제 메서드
    public void DestroyBuilding()
    {
        if (InBuildingRoot != null)
            Destroy(InBuildingRoot);  // 루트 삭제
        Destroy(gameObject);
    }
}