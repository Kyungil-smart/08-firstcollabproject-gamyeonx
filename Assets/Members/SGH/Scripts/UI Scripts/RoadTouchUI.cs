using UnityEngine;

public class RoadTouchUI : MonoBehaviour
{
    [SerializeField] private GameObject _uiBuildPanel;
    [SerializeField] private GameObject _roadTouchUIPanel;

    public void OnClicSetRoadTouchUI()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiBuildPanel.SetActive(false);
        _roadTouchUIPanel.SetActive(true);
    }
    public void OnClicCloseRoadTouchUI()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiBuildPanel.SetActive(true);
        _roadTouchUIPanel.SetActive(false);
    }
    public void OnClicBackRoadTouchUI()
    {
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiBuildPanel.SetActive(true);
        _roadTouchUIPanel.SetActive(false);
    }

}
