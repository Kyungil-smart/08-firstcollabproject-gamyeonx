using UnityEngine;
using UnityEngine.UI;

public class RestaurantBackButton : MonoBehaviour
{
    [SerializeField] private GameObject _buildPanel;
    [SerializeField] private GameObject _uiRestaurantBuildPanel;
    [SerializeField] private GameObject _restaurantBuildButton;
    [SerializeField] private GameObject _topUI;

    private Button _restaurantbackButton;

    private void Awake()
    {
        _restaurantbackButton = GetComponent<Button>();
        if (_restaurantbackButton != null) _restaurantbackButton.onClick.AddListener(OnClickBackButton);
    }

    public void OnClickBackButton()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _restaurantBuildButton.SetActive(true);
        _topUI.SetActive(true);
        _uiRestaurantBuildPanel.SetActive(false);
        _buildPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_restaurantbackButton != null) _restaurantbackButton.onClick.RemoveListener(OnClickBackButton);
    }
}
