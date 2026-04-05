using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingClick : MonoBehaviour, IPointerClickHandler
{

    // Physics 2D Raycaster 있어야 작동함.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIManager.Instance.OpenMenu == true) return;
        GridBuildingSystem.Instance.OnClickSetFurnitureMenu(this.gameObject);
        UIManager.Instance.OpenMenu = true;
        UIManager.Instance._topUIPanel.SetActive(false);
        UIManager.Instance._restaurantBuildButton.SetActive(false);
        UIManager.Instance._hotSpringBuildButton.SetActive(false);
        UIManager.Instance._shopBuildButton.SetActive(false);
        UIManager.Instance._trainingGround_Build_Button.SetActive(false);
        UIManager.Instance._vendingMachineBuildButton.SetActive(false);
        Time.timeScale = 0f;
    }
}
