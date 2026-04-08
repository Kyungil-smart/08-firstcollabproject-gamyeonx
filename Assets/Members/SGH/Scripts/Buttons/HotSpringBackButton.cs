using UnityEngine;
using UnityEngine.UI;

public class HotSpringBackButton : MonoBehaviour
{
    [SerializeField] private GameObject _buildPanel;
    [SerializeField] private GameObject _uiHotSpringBuildPanel;
    [SerializeField] private GameObject _hotSpringBuildButton;
    [SerializeField] private GameObject _topUI;

    private Button _hotSpringbackButton;

    private void Awake()
    {
        _hotSpringbackButton = GetComponent<Button>();
        if (_hotSpringbackButton != null) _hotSpringbackButton.onClick.AddListener(OnClickBackButton);
    }

    public void OnClickBackButton()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _hotSpringBuildButton.SetActive(true);
        _topUI.SetActive(true);
        _uiHotSpringBuildPanel.SetActive(false);
        _buildPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_hotSpringbackButton != null) _hotSpringbackButton.onClick.RemoveListener(OnClickBackButton);
    }
}
