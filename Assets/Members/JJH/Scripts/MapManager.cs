using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    
    [Header("외부 건물 생성")]
    [SerializeField] private int CurrentBuildingNum = 0;
    [SerializeField] private float BuildingPivotDistance = 5f; // 빌딩 생성용 임시 함수 추후 제거

    [Header("내부 건물 생성")] 
    [Header("Level1")] 
    [SerializeField] private GameObject Guild;
    [SerializeField] private GameObject HotSpring;
    [SerializeField] private GameObject Restaurant;
    [SerializeField] private GameObject VendingMachine;
    [SerializeField] private GameObject Shop;
    [SerializeField] private GameObject TrainingGround;
    
    // [Header("Level2")] 
    // [SerializeField] private GameObject HotSpringLevel2;
    // [SerializeField] private GameObject RestaurantLevel2;
    // [SerializeField] private GameObject VendingMachineLevel2;
    // [SerializeField] private GameObject ShopLevel2;
    // [SerializeField] private GameObject TrainingGroundLevel2;
    //
    // [Header("Level3")] 
    // [SerializeField] private GameObject HotSpringLevel3;
    // [SerializeField] private GameObject RestaurantLevel3;
    // [SerializeField] private GameObject VendingMachineLevel3;
    // [SerializeField] private GameObject ShopLevel3;
    // [SerializeField] private GameObject TrainingGroundLevel3;
    
    [SerializeField] private GameObject InBuildingPivot;
    [SerializeField] private float InBuildingPivotDistance = 50f;
    [SerializeField] private int CurrentInBuildingNum = 0;
    
    private List<GameObject> InBuildingPrefabs;

    public int MapLevel = 1;

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

    private void Start()
    {
        PrefabListAdd();
    }

    public void InstantiateInBuilding(Building building, int index)
    {
        if (index < 0) return;
        CurrentBuildingNum++;
        
        // 내부 건물 생성 로직
        Vector2 InBuildingInstantiatePivot = PivotTransform(InBuildingPivot, CurrentInBuildingNum, InBuildingPivotDistance);
        GameObject inBuilding = Instantiate(InBuildingPrefabs[index], InBuildingInstantiatePivot, InBuildingPivot.transform.rotation);
        CurrentInBuildingNum++;
        
        InBuildingData inData = inBuilding.GetComponentInChildren<InBuildingData>();
        building.InBuildingData = inData;
        building.InBuildingRoot = inBuilding;
        
        FacilityRuntime facilityRuntime = building.GetComponentInChildren<FacilityRuntime>();
        if (facilityRuntime != null && inData != null)
        {
            facilityRuntime._inBuildingData = inData;
            // 연동준이 추가
            inData.SetFacilityRuntime(facilityRuntime);
            FacilityRegistry.Instance?.RegisterFacility(facilityRuntime);
        }
    }
    
    private Vector2 PivotTransform(GameObject obj, int curInstance, float distance)
    {
        float x = obj.transform.position.x + (curInstance * distance);
        float y = obj.transform.position.y;
        return new Vector2(x, y);
    }

    private void PrefabListAdd()
    {
        InBuildingPrefabs = new List<GameObject>();
        
        InBuildingPrefabs.Add(Guild);
        InBuildingPrefabs.Add(HotSpring);
        InBuildingPrefabs.Add(Restaurant);
        InBuildingPrefabs.Add(VendingMachine);
        InBuildingPrefabs.Add(Shop);
        InBuildingPrefabs.Add(TrainingGround);
    }
}
