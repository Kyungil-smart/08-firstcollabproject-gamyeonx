using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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

    public void BuildingEntered()
    {
        _canvas.gameObject.SetActive(true);
    }

    public void BuildingLevelUp()
    {
        if (maxLevel <= currentLevel) return;
        float X = CameraPivot.transform.position.x;
        float Y = CameraPivot.transform.position.y;
        Vector2 NextLevelPivot = new Vector2(X, Y);
        Instantiate(_nextLevelPrefabs[currentLevel - 1], NextLevelPivot, CameraPivot.transform.rotation);
        currentLevel++;
    }

    public void ReturnButton()
    {
        _cameraController.ReturnToWorld();
        _canvas.gameObject.SetActive(false);
    }
}
