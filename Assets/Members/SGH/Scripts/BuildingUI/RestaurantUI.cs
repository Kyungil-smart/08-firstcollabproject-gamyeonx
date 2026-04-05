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
        UIManager.Instance._buildButton.SetActive(true);
        UIManager.Instance._topUIPanel.SetActive(true);
        UIManager.Instance.OpenMenu = false;
        Time.timeScale = 1f;
    }

    public void OnclickDemolish()
    {
        UIManager.Instance.OnClickSetBuildDoublecheckPanel();
        UIManager.Instance._buildButton.SetActive(true);
        UIManager.Instance._topUIPanel.SetActive(true);
        UIManager.Instance.OpenMenu = false;
    }
}
