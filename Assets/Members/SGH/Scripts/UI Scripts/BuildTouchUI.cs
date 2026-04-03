using Unity.VisualScripting;
using UnityEngine;

public class BuildTouchUI : MonoBehaviour
{
    [SerializeField] private GameObject _uiBuildPanel;
    [SerializeField] private GameObject _buildTouchUIPanel;
    [SerializeField] private GridBuildingSystem _gridBuildingSystem;

    public void OnClicSetBuildTouchUI()
    {
        if (_gridBuildingSystem.GoldAmount > GoldTest.Instance._testGold)
        {
            Debug.Log($"[BuildTouchUI]골드가 부족합니다 {_gridBuildingSystem.GoldAmount}");
            return;
        }

        if (_gridBuildingSystem.UnlockRevenue > GoldTest.Instance.IncreasedGold)
        {
            Debug.Log($"[BuildTouchUI]누적 비용이 부족합니다 {_gridBuildingSystem.UnlockRevenue}");
            return;
        }
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiBuildPanel.SetActive(false);
        _buildTouchUIPanel.SetActive(true);
        
    }
    public void OnClicCloseBuildTouchUI()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiBuildPanel.SetActive(true);
        _buildTouchUIPanel.SetActive(false);
    }
    public void OnClicBackBuildTouchUI()
    {
        if (GridBuildingSystem.Instance._temp != null && !GridBuildingSystem.Instance._temp.Placed)
        {
            // 아직 설치되지 않은 건물
            GridBuildingSystem.Instance.CancelPlacement();
        }
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiBuildPanel.SetActive(true);
        _buildTouchUIPanel.SetActive(false);
    }

}
