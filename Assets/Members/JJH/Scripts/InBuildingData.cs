using System;
using UnityEngine;
using UnityEngine.UI;

public class InBuildingData : MonoBehaviour
{
    public GameObject Pivot;
    
    private Button _returnButton;
    private Canvas _canvas;

    private void Awake()
    {
        _canvas.gameObject.SetActive(true);
    }

    private void InstantiatePivot()
    {
        
    }

    public void ReturnButton()
    {
        // 원래 위치로 돌아가는 로직 추가
        _canvas.gameObject.SetActive(false);
    }
}
