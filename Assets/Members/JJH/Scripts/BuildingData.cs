using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingData : MonoBehaviour
{
    public Canvas Canvas;
    public Button Enter;
    public Button LevelUp;
    CameraController _cameraController;
    
    public InBuildingData InBuildingData;

    private void Awake()
    {
        Canvas.gameObject.SetActive(false);
    }
    
    private void Start()
    {
        _cameraController = FindFirstObjectByType<CameraController>();
        Enter.onClick.AddListener(EnterBuilding);
        LevelUp.onClick.AddListener(LevelUpBuilding);
    }

    public void CanvasActive()
    {
        _cameraController.SetInputLock(true);
        Canvas.gameObject.SetActive(true);
    }

    public void EnterBuilding()
    {
        _cameraController.MoveToBuilding(
            pivot      : InBuildingData.Pivot.transform,
            boundsSize : new Vector2(InBuildingData.CurLevel * 10, InBuildingData.CurLevel * 10), 
            minSize    : InBuildingData.CurLevel * 2f,
            maxSize    : InBuildingData.CurLevel * 6f
        );
        
        Canvas.gameObject.SetActive(false);
        InBuildingData.EnterBuilding();
        _cameraController.SetInputLock(false);
    }

    public void LevelUpBuilding()
    {
        // 재화 있고 최대레벨 이하면 내부 건물 스크립트의 Levelup 불러오기
        // if (현재재화 < 필요재화) return;
        InBuildingData.BuildingLevelUp();
        Canvas.gameObject.SetActive(false);
        _cameraController.SetInputLock(false);
    }
}
