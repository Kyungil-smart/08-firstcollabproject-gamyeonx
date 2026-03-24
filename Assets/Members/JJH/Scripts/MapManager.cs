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
    public float InBuildingPivotDistance = 50f;
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
        Vector2 InstatiatePivot = PivotTransform(InstancePivot, CurrentInstance, PivotDistance);
        Instantiate(BuildingPrefab, InstatiatePivot, InstancePivot.transform.rotation);
        CurrentInstance++;
        
        // 내부 건물 생성 로직
        Vector2 InBuildingInstatiatePivot = PivotTransform(InBuildingPivot, CurrentInBuilding, InBuildingPivotDistance);
        Instantiate(InBuildingPrefab1, InBuildingInstatiatePivot, InstancePivot.transform.rotation);
        CurrentInBuilding++;
    }
    
    private Vector2 PivotTransform(GameObject obj, int curInstance, float distance)
    {
        float X = obj.transform.position.x + (curInstance * distance);
        float Y = obj.transform.position.y;
        return new Vector2(X, Y);
    }
}
