using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    
    [Header("외부 건물 생성")]
    public List<GameObject> BuildingPrefabs;
    public GameObject BuildingPivot;
    public int CurrentBuildingNum = 0;
    public float BuildingPivotDistance = 5f; // 빌딩 생성용 임시 함수 추후 제거

    [Header("내부 건물 생성")] 
    public List<GameObject> InBuildingPrefabs;
    public GameObject InBuildingPivot;
    public float InBuildingPivotDistance = 50f;
    public int CurrentInBuildingNum = 0;

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
        
        // 외부 건물 생성 로직 (추후 그리드시스템과 연계)
        Vector2 BuildingInstatiatePivot = PivotTransform(BuildingPivot, CurrentBuildingNum, BuildingPivotDistance);
        GameObject outBuilding = Instantiate(BuildingPrefabs[index], BuildingInstatiatePivot, BuildingPivot.transform.rotation);
        CurrentBuildingNum++;
        
        // 내부 건물 생성 로직
        Vector2 InBuildingInstantiatePivot = PivotTransform(InBuildingPivot, CurrentInBuildingNum, InBuildingPivotDistance);
        GameObject inBuilding = Instantiate(InBuildingPrefabs[index], InBuildingInstantiatePivot, BuildingPivot.transform.rotation);
        CurrentInBuildingNum++;

        BuildingData outData = outBuilding.GetComponentInChildren<BuildingData>();
        InBuildingData inData = inBuilding.GetComponentInChildren<InBuildingData>();
        outData.InBuildingData = inData;
    }
    
    private Vector2 PivotTransform(GameObject obj, int curInstance, float distance)
    {
        float x = obj.transform.position.x + (curInstance * distance);
        float y = obj.transform.position.y;
        return new Vector2(x, y);
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
