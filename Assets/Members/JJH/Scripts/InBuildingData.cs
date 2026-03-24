using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InBuildingData : MonoBehaviour
{
    [Header("카메라 기준점")]
    public GameObject Pivot;
    
    [Header("캔버스 및 버튼")]   
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _returnButton;

    [Header("건물 정보")]
    public int CurLevel = 1;
    public int MaxLevel = 2;
    
    [Header("다음레벨 프리팹")]
    [SerializeField] private List<GameObject> _nextLevelPrefabs;
    
    CameraController _cameraController;

    private void Awake()
    {
        _canvas.gameObject.SetActive(false);
    }

    private void Start()
    {
        _returnButton.onClick.AddListener(ReturnButton);
        _cameraController = FindFirstObjectByType<CameraController>();
    }

    public void EnterBuilding()
    {
        _canvas.gameObject.SetActive(true);
    }

    public void BuildingLevelUp()
    {
        if (MaxLevel <= CurLevel) return;
        float X = Pivot.transform.position.x;
        float Y = Pivot.transform.position.y;
        Vector2 NextLevelPivot = new Vector2(X, Y);
        Instantiate(_nextLevelPrefabs[CurLevel - 1], NextLevelPivot, Pivot.transform.rotation);
        CurLevel++;
    }

    public void ReturnButton()
    {
        _cameraController.ReturnToWorld();
        _canvas.gameObject.SetActive(false);
    }
}
