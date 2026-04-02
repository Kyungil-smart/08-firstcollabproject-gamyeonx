using UnityEngine;
using UnityEngine.UI;

public class VendingMachineBuildButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiVendingMachineBuildPanel;
    [SerializeField] private GameObject _topUI;
    [SerializeField] private EFacilityType _buttonType;

    private Button _vendingMachineButton;

    private void Awake()
    {
        _vendingMachineButton = GetComponent<Button>();

        if (_vendingMachineButton != null) _vendingMachineButton.onClick.AddListener(OnClickSetRestaurantBuildPanel);
    }

    public void OnClickSetRestaurantBuildPanel()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiVendingMachineBuildPanel.SetActive(true);
        _topUI.SetActive(false);
        gameObject.SetActive(false);
    }

    public EFacilityType GetButtonType()
    {
        return _buttonType;
    }

    private void OnDestroy()
    {
        if (_vendingMachineButton != null) _vendingMachineButton.onClick.RemoveListener(OnClickSetRestaurantBuildPanel);
    }
}
