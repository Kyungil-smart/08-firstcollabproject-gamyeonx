using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingData : MonoBehaviour
{
    public Canvas Canvas;
    public Button Enter;
    public Button LevelUp;
    CameraController _cameraController;

    private void Awake()
    {
        Canvas.gameObject.SetActive(false);
    }

    public void Start()
    {
        Enter.onClick.AddListener(EnterAddListner);
        LevelUp.onClick.AddListener(LevelUpAddListner);
    }

    public void CanvasActive()
    {
        _cameraController.SetInputLock(true);
        Canvas.gameObject.SetActive(true);
    }

    void EnterAddListner()
    {
        CameraController cam = FindObjectOfType<CameraController>();
        cam.MoveToBuilding(
            pivot      : transform,  // 추후 내부건물로 변경해야함
            boundsSize : new Vector2(10, 10), 
            minSize    : 2f,
            maxSize    : 6f // 사이즈 관련도 내부 건물 레벨에 따라 변경 필요
        );
        
        Canvas.gameObject.SetActive(false);
    }

    void LevelUpAddListner()
    {
        // 재화 있고 최대레벨 이하면 내부 건물 스크립트의 Levelup 불러오기
        Canvas.gameObject.SetActive(false);
    }
}
