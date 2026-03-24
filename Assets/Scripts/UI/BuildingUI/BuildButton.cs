using UnityEngine;
using UnityEngine.UI;

public class BuildButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiBuildPanel;
    [SerializeField] private GameObject _topUI;

    private Button _buildButton;

    private void Awake()
    {
        _buildButton = GetComponent<Button>();

        if (_buildButton != null) _buildButton.onClick.AddListener(OnClickSetBuildPanel);
    }

    public void OnClickSetBuildPanel()
    {
        UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiBuildPanel.SetActive(true);
        _topUI.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_buildButton != null) _buildButton.onClick.RemoveListener(OnClickSetBuildPanel);
    }
}
