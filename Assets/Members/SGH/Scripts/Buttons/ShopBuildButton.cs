using UnityEngine;
using UnityEngine.UI;

public class ShopBuildButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiShopBuildPanel;
    [SerializeField] private GameObject _topUI;

    private Button _shopButton;

    private void Awake()
    {
        _shopButton = GetComponent<Button>();

        if (_shopButton != null) _shopButton.onClick.AddListener(OnClickSetRestaurantBuildPanel);
    }

    public void OnClickSetRestaurantBuildPanel()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiShopBuildPanel.SetActive(true);
        _topUI.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_shopButton != null) _shopButton.onClick.RemoveListener(OnClickSetRestaurantBuildPanel);
    }
}
