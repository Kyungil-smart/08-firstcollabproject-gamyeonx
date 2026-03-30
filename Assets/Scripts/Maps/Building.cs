using UnityEngine;
using UnityEngine.UI;

public enum BuildType
{
    Road,
    Building,
    TileBrush
}

public enum ETurnType
{
    SquareTurn,
    TwoxFourTurn,
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

    // 회전 전용 필드
    public ETurnType turnType; // 회전 방식
    public Transform pivot; // 회전용 피벗
    public int rotateCount = 0; // 회전 횟수 카운트

    [Header("해당 건물과 건축버튼 매칭용")]
    [SerializeField] private EFacilityType _facilityType;

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

        UIManager.Instance._buildButton.SetActive(false);
        UIManager.Instance.SetFurnitureButtonActive(_facilityType, true);

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

    //--- 회전 로직 ---

    // 오브젝트 회전시키는 메서드
    public void Rotate()
    {
        if (Placed) return;

        switch (turnType)
        {
            case ETurnType.SquareTurn: RotateSquareBuilding(); break;
            case ETurnType.TwoxFourTurn: RotateTwoxFourBuilding(); break;
                // 필요시 다른 타입 추가 및 함수 추가 제작 필요할수도 있음
        }
    }

    // 정사각형용 회전
    private void RotateSquareBuilding()
    {
        pivot.Rotate(0, 0, 90);
        rotateCount = (rotateCount + 1) % 4;

        var size = area.size;
        area.size = new Vector3Int(size.y, size.x, size.z);
    }

    // 여기서 부터는 추후 만들어질 건물의 크기에 맞게 함수를 따로 만들어야함.

    private void RotateTwoxFourBuilding()
    {
        if (Placed) return;

        pivot.Rotate(0, 0, 90);

        var size = area.size;
        area.size = new Vector3Int(size.y, size.x, size.z);

        rotateCount = (rotateCount + 1) % 4;

        Vector3 offset = Vector3.zero;
        switch (rotateCount)
        {
            case 1:
                offset = new Vector3(0, -1, 0);
                break;
            case 2:
                offset = new Vector3(-2, -1, 0);
                break;
            case 3:
                offset = new Vector3(-2, -3, 0);
                break;
            case 0:
                offset = new Vector3(0, -3, 0);
                break;
        }

        pivot.Translate(offset, Space.Self);
    }

    //------------

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