using UnityEngine;
using UnityEngine.EventSystems;

public class RoadClick : MonoBehaviour, IPointerClickHandler
{
    // Physics 2D Raycaster 있어야 작동함.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIManager.Instance.OpenMenu == true) return;
        GridBuildingSystem.Instance.OnClickSetRoadMenu(this.gameObject);
        // UIManager.Instance.OpenMenu = true;
    }
}
