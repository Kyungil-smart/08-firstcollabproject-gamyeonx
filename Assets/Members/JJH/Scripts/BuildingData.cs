using UnityEngine;
using UnityEngine.UI;

public class BuildingData : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _enterButton;
    [SerializeField] private Button _levelUpButton;
    CameraController _cameraController;
    
    public InBuildingData InBuildingData;

    private void Awake()
    {
        _canvas.gameObject.SetActive(false);
    }
    
    private void Start()
    {
        _cameraController = FindFirstObjectByType<CameraController>();
        _enterButton.onClick.AddListener(BuildingEntered);
        _levelUpButton.onClick.AddListener(BuildingLevelUp);
    }

    public void CanvasActive()
    {
        _cameraController.SetInputLock(true);
        _canvas.gameObject.SetActive(true);
    }

    public void BuildingEntered()
    {
        _cameraController.MoveToBuilding(
            pivot      : InBuildingData.CameraPivot.transform,
            boundsSize : new Vector2(InBuildingData.CurrentLevel * 10, InBuildingData.CurrentLevel * 10), 
            minSize    : InBuildingData.CurrentLevel * 2f,
            maxSize    : InBuildingData.CurrentLevel * 6f
        );
        
        _canvas.gameObject.SetActive(false);
        InBuildingData.BuildingEntered();
        _cameraController.SetInputLock(false);
    }

    public void BuildingLevelUp()
    {
        // 재화 있고 최대레벨 이하면 내부 건물 스크립트의 Levelup 불러오기
        // if (현재재화 < 필요재화) return;
        InBuildingData.BuildingLevelUp();
        _canvas.gameObject.SetActive(false);
        _cameraController.SetInputLock(false);
    }
}
