using UnityEngine;
using UnityEngine.UI;

public class HotSpringBuildButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiHotSpringPanel;
    [SerializeField] private GameObject _topUI;
    [SerializeField] private EFacilityType _buttonType;

    private Button _hotSpringbuildButton;

    private void Awake()
    {
        _hotSpringbuildButton = GetComponent<Button>();

        if (_hotSpringbuildButton != null) _hotSpringbuildButton.onClick.AddListener(OnClickSetRestaurantBuildPanel);
    }

    public void OnClickSetRestaurantBuildPanel()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiHotSpringPanel.SetActive(true);
        _topUI.SetActive(false);
        gameObject.SetActive(false);
    }

    public EFacilityType GetButtonType()
    {
        return _buttonType;
    }

    private void OnDestroy()
    {
        if (_hotSpringbuildButton != null) _hotSpringbuildButton.onClick.RemoveListener(OnClickSetRestaurantBuildPanel);
    }
}
