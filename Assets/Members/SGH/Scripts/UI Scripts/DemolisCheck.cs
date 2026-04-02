using UnityEngine;

public class DemolisCheck : MonoBehaviour
{
    [SerializeField] private GameObject DemolisCheckPanel;

    public void OnClickSetDemolisCheckPanelUI()
    {
        DemolisCheckPanel.SetActive(true);
    }

    public void OnClickCloseDemolisCheckPanelUI()
    {
        DemolisCheckPanel.SetActive(false);
    }


}
