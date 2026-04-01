using UnityEngine;

public class RestaurantUI : MonoBehaviour
{
    [SerializeField] private GameObject _restaurantUI;

    public void OnclickBack()
    {
        _restaurantUI.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null)
        {
            GridBuildingSystem.Instance._temp.IsMenuOpen = false;
            GridBuildingSystem.Instance._temp = null;
        }
    }
}
