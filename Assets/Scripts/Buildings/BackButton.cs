using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiBuildPanel;
    [SerializeField] private GameObject _BuildButton;

    private Button _backButton;

    private void Awake()
    {
        _backButton = GetComponent<Button>();
        if (_backButton != null) _backButton.onClick.AddListener(OnClickBackButton);
    }

    public void OnClickBackButton()
    {
        UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _BuildButton.SetActive(true);
        _uiBuildPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_backButton != null) _backButton.onClick.RemoveListener(OnClickBackButton);
    }
}
