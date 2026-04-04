using UnityEngine;

public class RoadTouchUI : MonoBehaviour
{
    [SerializeField] private GameObject _uiBuildPanel;
    [SerializeField] private GameObject _roadTouchUIPanel;
    [SerializeField] private GridBuildingSystem _gridBuildingSystem;

    public void OnClicSetRoadTouchUI()
    {
        if (_gridBuildingSystem.GoldAmount > GoldTest.Instance._testGold)
        {
            Debug.Log($"[BuildTouchUI]골드가 부족합니다 {_gridBuildingSystem.GoldAmount}");
            return;
        }

        if (!_gridBuildingSystem.IsCanPlacing)
        {
            Debug.Log("[BuildTouchUI]수용성 가구가 3개를 초과했습니다.");
            return;
        }
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
