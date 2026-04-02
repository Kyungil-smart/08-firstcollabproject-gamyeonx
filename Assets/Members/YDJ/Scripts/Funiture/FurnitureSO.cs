using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FurnitureSO", menuName = "Scriptable Objects/FurnitureSO")]
public class FurnitureSO : ScriptableObject
{
    public List<FurnitureData> furnituresDatas = new List<FurnitureData>();
}
