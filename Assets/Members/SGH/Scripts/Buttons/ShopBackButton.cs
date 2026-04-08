using UnityEngine;
using UnityEngine.UI;

public class ShopBackButton : MonoBehaviour
{
    [SerializeField] private GameObject _buildPanel;
    [SerializeField] private GameObject _uiShopBuildPanel;
    [SerializeField] private GameObject _shopBuildButton;
    [SerializeField] private GameObject _topUI;

    private Button _shopbackButton;

    private void Awake()
    {
        _shopbackButton = GetComponent<Button>();
        if (_shopbackButton != null) _shopbackButton.onClick.AddListener(OnClickBackButton);
    }

    public void OnClickBackButton()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _shopBuildButton.SetActive(true);
        _topUI.SetActive(true);
        _uiShopBuildPanel.SetActive(false);
        _buildPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_shopbackButton != null) _shopbackButton.onClick.RemoveListener(OnClickBackButton);
    }
}
