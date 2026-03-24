using System;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    
    [Header("외부 건물 생성")]
    public GameObject BuildingPrefab;
    public GameObject InstancePivot;
    public int CurrentInstance = 0;
    public float PivotDistance = 5f;

    [Header("내부 건물 생성")] 
    public GameObject InBuildingPrefab1;
    public GameObject InBuildingPrefab2;
    public GameObject InBuildingPivot;
    public float InBuildingPivotDistance = 20f;
    public int CurrentInBuilding = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void InstantiateButton()
    {
        // 외부 건물 생성 로직
        float X = InstancePivot.transform.position.x + (CurrentInstance * PivotDistance);
        float Y = InstancePivot.transform.position.y;
        Vector2 InstatiatePivot = new Vector2(X, Y);
        Instantiate(BuildingPrefab, InstatiatePivot, InstancePivot.transform.rotation);
        CurrentInstance++;
        
        // 내부 건물 생성 함수
        InBuildingInstatntiate();
    }

    private void InBuildingInstatntiate()
    {
        float X = InBuildingPivot.transform.position.x + (CurrentInBuilding * InBuildingPivotDistance);
        float Y = InBuildingPivot.transform.position.y;
        Vector2 InBuildingInstatiatePivot = new Vector2(X, Y);
        Instantiate(BuildingPrefab, InBuildingInstatiatePivot, InBuildingPivot.transform.rotation);
        CurrentInBuilding++;
    }
}
