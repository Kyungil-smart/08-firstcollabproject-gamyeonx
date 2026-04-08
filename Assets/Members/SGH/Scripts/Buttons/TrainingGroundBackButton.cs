using UnityEngine;
using UnityEngine.UI;
public class TrainingGroundBackButton : MonoBehaviour
{
    [SerializeField] private GameObject _buildPanel;
    [SerializeField] private GameObject _uiTrainingGroundBuildPanel;
    [SerializeField] private GameObject _trainingGroundBuildButton;
    [SerializeField] private GameObject _topUI;

    private Button _trainingGroundbackButton;

    private void Awake()
    {
        _trainingGroundbackButton = GetComponent<Button>();
        if (_trainingGroundbackButton != null) _trainingGroundbackButton.onClick.AddListener(OnClickBackButton);
    }

    public void OnClickBackButton()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _trainingGroundBuildButton.SetActive(true);
        _topUI.SetActive(true);
        _uiTrainingGroundBuildPanel.SetActive(false);
        _buildPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_trainingGroundbackButton != null) _trainingGroundbackButton.onClick.RemoveListener(OnClickBackButton);
    }
}
