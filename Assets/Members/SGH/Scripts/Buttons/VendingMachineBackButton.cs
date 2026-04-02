using UnityEngine;
using UnityEngine.UI;

public class VendingMachineBackButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiVendingMachineBuildPanel;
    [SerializeField] private GameObject _vendingMachineBuildButton;
    [SerializeField] private GameObject _topUI;

    private Button _vendingMachinebackButton;

    private void Awake()
    {
        _vendingMachinebackButton = GetComponent<Button>();
        if (_vendingMachinebackButton != null) _vendingMachinebackButton.onClick.AddListener(OnClickBackButton);
    }

    public void OnClickBackButton()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _vendingMachineBuildButton.SetActive(true);
        _topUI.SetActive(true);
        _uiVendingMachineBuildPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_vendingMachinebackButton != null) _vendingMachinebackButton.onClick.RemoveListener(OnClickBackButton);
    }
}
