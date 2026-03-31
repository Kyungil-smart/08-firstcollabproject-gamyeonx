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
    public void OnClicBackBuildTouchUI()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiBuildPanel.SetActive(true);
        _buildTouchUIPanel.SetActive(false);
    }

}
