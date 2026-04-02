using Unity.VisualScripting;
using UnityEngine;

public class BuildTouchUI : MonoBehaviour
{
    [SerializeField] private GameObject _uiBuildPanel;
    [SerializeField] private GameObject _buildTouchUIPanel;

    public void OnClicSetBuildTouchUI()
    {
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
