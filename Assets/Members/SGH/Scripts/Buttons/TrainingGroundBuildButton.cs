using UnityEngine;
using UnityEngine.UI;

public class TrainingGroundBuildButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiTrainingGroundBuildPanel;
    [SerializeField] private GameObject _topUI;

    private Button _trainingGroundButton;

    private void Awake()
    {
        _trainingGroundButton = GetComponent<Button>();

        if (_trainingGroundButton != null) _trainingGroundButton.onClick.AddListener(OnClickSetRestaurantBuildPanel);
    }

    public void OnClickSetRestaurantBuildPanel()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiTrainingGroundBuildPanel.SetActive(true);
        _topUI.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_trainingGroundButton != null) _trainingGroundButton.onClick.RemoveListener(OnClickSetRestaurantBuildPanel);
    }
}
