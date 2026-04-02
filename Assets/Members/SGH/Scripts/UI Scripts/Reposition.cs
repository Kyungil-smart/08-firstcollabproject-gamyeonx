using UnityEngine;

public class Reposition : MonoBehaviour
{
    [SerializeField] private GameObject _touchUiPanel;
    [SerializeField] private GameObject _demolishUiPanel;

    public void OnClickRepositionUI()
    {
        _demolishUiPanel.SetActive(false);
        _touchUiPanel.SetActive(true);
    }

    public void OnClickBackUI()
    {
        _demolishUiPanel.SetActive(false);
    }
}
