using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingClick : MonoBehaviour, IPointerClickHandler
{

    // Physics 2D Raycaster 있어야 작동함.
    public void OnPointerClick(PointerEventData eventData)
    {
        GridBuildingSystem.Instance.OnClickSetFurnitureMenu(this.gameObject);
    }
}
