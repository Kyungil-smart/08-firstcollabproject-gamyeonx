using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    
    [Header("외부 건물 생성")]
    public List<GameObject> BuildingPrefabs;
    public GameObject InstancePivot;
    public int CurrentInstance = 0;
    public float PivotDistance = 5f;

    [Header("내부 건물 생성")] 
    public List<GameObject> InBuildingPrefabs;
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
        int index = 0; // 추후 UI쪽 스크립트에서 함수로 받아올것
        
        // 외부 건물 생성 로직
        Vector2 InstatiatePivot = PivotTransform(InstancePivot, CurrentInstance, PivotDistance);
        GameObject outBuilding = Instantiate(BuildingPrefabs[index], InstatiatePivot, InstancePivot.transform.rotation);
        CurrentInstance++;
        
        // 내부 건물 생성 로직
        Vector2 InBuildingInstatiatePivot = PivotTransform(InBuildingPivot, CurrentInBuilding, InBuildingPivotDistance);
        GameObject inBuilding = Instantiate(InBuildingPrefabs[index], InBuildingInstatiatePivot, InstancePivot.transform.rotation);
        CurrentInBuilding++;

        BuildingData outData = outBuilding.GetComponentInChildren<BuildingData>();
        InBuildingData inData = inBuilding.GetComponentInChildren<InBuildingData>();
        outData.InBuildingData = inData;
    }
    
    private Vector2 PivotTransform(GameObject obj, int curInstance, float distance)
    {
        float X = obj.transform.position.x + (curInstance * distance);
        float Y = obj.transform.position.y;
        return new Vector2(X, Y);
    }

    // 추후 UI 쪽 스크립트로 이전
    public int BuildingIndex(GameObject obj)
    {
        switch (obj.tag)
        {
            case "HotSpring":
                return 0;
            default:
                return -1;
        }
    }
}
