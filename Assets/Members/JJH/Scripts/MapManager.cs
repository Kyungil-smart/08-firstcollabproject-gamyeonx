using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    
    [Header("외부 건물 생성")]
    // [SerializeField] private List<GameObject> BuildingPrefabs;
    // [SerializeField] private GameObject BuildingPivot;
    [SerializeField] private int CurrentBuildingNum = 0;
    [SerializeField] private float BuildingPivotDistance = 5f; // 빌딩 생성용 임시 함수 추후 제거

    [Header("내부 건물 생성")] 
    [SerializeField] private List<GameObject> InBuildingPrefabs;
    [SerializeField] private GameObject InBuildingPivot;
    [SerializeField] private float InBuildingPivotDistance = 50f;
    [SerializeField] private int CurrentInBuildingNum = 0;

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
    
    public void InstantiateInBuilding(Building building, int index)
    {
        /*int index = 0; // 추후 UI쪽 스크립트에서 함수로 받아올것
        
        // 외부 건물 생성 로직 (추후 그리드시스템과 연계)
        Vector2 BuildingInstatiatePivot = PivotTransform(BuildingPivot, CurrentBuildingNum, BuildingPivotDistance);
        GameObject outBuilding = Instantiate(BuildingPrefabs[index], BuildingInstatiatePivot, BuildingPivot.transform.rotation);*/

        if (index < 0) return;
        CurrentBuildingNum++;
        
        // 내부 건물 생성 로직
        Vector2 InBuildingInstantiatePivot = PivotTransform(InBuildingPivot, CurrentInBuildingNum, InBuildingPivotDistance);
        GameObject inBuilding = Instantiate(InBuildingPrefabs[index], InBuildingInstantiatePivot, InBuildingPivot.transform.rotation);
        CurrentInBuildingNum++;
        
        InBuildingData inData = inBuilding.GetComponentInChildren<InBuildingData>();
        building.InBuildingData = inData;
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
