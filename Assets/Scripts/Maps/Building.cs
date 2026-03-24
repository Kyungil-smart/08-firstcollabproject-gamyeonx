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

    // 오브젝트 회전시키는 메서드
    public void Rotate()
    {
        transform.Rotate(0, 0, -90);

        var size = area.size;
        area.size = new Vector3Int(size.y, size.x, size.z);
    }

    // 오브젝트 재배치용 메서드
    public void StartMove()
    {
        if (!Placed) return;
        GridBuildingSystem.Instance.ReleaseArea(area);
        Placed = false;
    }
}