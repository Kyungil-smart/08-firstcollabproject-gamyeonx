using System;

[Serializable]
public class FurnitureData
{
    public string interiorID;
    public string interiorNameKo;
    public string interiorNameEn;

    public BuildType interiorType;
    public string interiorTargetFacility;

    public int interiorPrice;
    public int interiorCapacityGrowth;
    public int interiorFeeGrowth;
}
