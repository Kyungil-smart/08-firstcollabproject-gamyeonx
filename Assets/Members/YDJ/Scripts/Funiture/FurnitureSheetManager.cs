using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureSheetManager : MonoBehaviour
{
    public static FurnitureSheetManager Instance { get; private set; }
    
    public FurnitureSheetData _furnitureSheet;
    
    [SerializeField] private FurnitureSO _furnitureSO;
    public List<FurnitureData> _furnitureDatas;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _furnitureDatas = new List<FurnitureData>(_furnitureSO.furnituresDatas);
        // _furnitureDatas = _furnitureSO.furnituresDatas;
        StartCoroutine(_furnitureSheet.Load(ParseFurnitureData));
    }
    
    private void ParseFurnitureData(char splitSymbol, string[] lines)
    {
        _furnitureDatas.Clear();

        if (lines == null || lines.Length <= 4)
        {
            Debug.LogError("Furniture 데이터 없음");
            return;
        }

        for (int i = 4; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;
            
            string[] cols = lines[i].Replace("\r", "").Split(splitSymbol);
            
            if (int.TryParse(cols[0], out _))
            {
                cols = cols[1..];
            }
            
            if (cols.Length < 8)
                continue;

            if (string.IsNullOrWhiteSpace(cols[0]) ||
                string.IsNullOrWhiteSpace(cols[1]))
                continue;
            
            FurnitureData data = new FurnitureData
            {
                interiorID = cols[0],
                interiorNameKo = cols[1],
                interiorNameEn = cols[2],
                interiorType = ParseEnumSafe(cols[3]),
                interiorTargetFacility = cols[4],

                interiorPrice = ParseIntSafe(cols[5]),
                interiorCapacityGrowth = ParseIntSafe(cols[6]),
                interiorFeeGrowth = ParseIntSafe(cols[7]),
            };

            _furnitureDatas.Add(data);
        }
    }
    
    private int ParseIntSafe(string value)
    {
        value = value.Trim();

        if (string.IsNullOrEmpty(value))
            return 0;

        if (int.TryParse(value, out int result))
            return result;

        Debug.LogError($"숫자 변환 실패: [{value}]");
        return 0;
    }

    private BuildType ParseEnumSafe(string value)
    {
        if (string.IsNullOrEmpty(value))
            return default;
        
        if (value == "capacity")
        {
            return BuildType.CapacityFurniture;
        }
        
        if (value == "fee")
        {
            return BuildType.FeeFurniture;
        }
        
        return default;
    }
    
    public List<FurnitureData> GetFurnitureByFacility(string facilityID)
    {
        return _furnitureDatas.FindAll(furnitureData =>
            furnitureData.interiorTargetFacility == facilityID);
    }
    
}