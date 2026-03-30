using UnityEngine;
using UnityEngine.UI;

public class RestaurantBuildButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiRestaurantPanel;
    [SerializeField] private GameObject _topUI;

    private Button _restaurantbuildButton;

    private void Awake()
    {
        _restaurantbuildButton = GetComponent<Button>();

        if (_restaurantbuildButton != null) _restaurantbuildButton.onClick.AddListener(OnClickSetRestaurantBuildPanel);
    }

    public void OnClickSetRestaurantBuildPanel()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiRestaurantPanel.SetActive(true);
        _topUI.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_restaurantbuildButton != null) _restaurantbuildButton.onClick.RemoveListener(OnClickSetRestaurantBuildPanel);
    }
}
