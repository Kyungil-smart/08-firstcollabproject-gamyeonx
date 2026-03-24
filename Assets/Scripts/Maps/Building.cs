using UnityEngine;

public class Building : MonoBehaviour
{
    public bool Placed { get; private set; }    // 설치 완료 여부
    public BoundsInt area;  // 건물이 차지하는 영역 오프셋

    // 현재 위치에서 설치 가능여부 체크용 메서드
    public bool CanbePlaced()
    {
        Vector3Int positionInt = GridBuildingSystem.Instance.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;

        if (GridBuildingSystem.Instance.CanTakeArea(areaTemp))
        {
            return true;
        }

        return false;
    }

    // 설치 처리용 메서드
    public void Place()
    {
        Vector3Int positionInt = GridBuildingSystem.Instance.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        Placed = true;
        GridBuildingSystem.Instance.TakeArea(areaTemp);
    }

    public void StartMove()
    {
        if (!Placed) return;
        GridBuildingSystem.Instance.ReleaseArea(area);
        Placed = false;
    }
}