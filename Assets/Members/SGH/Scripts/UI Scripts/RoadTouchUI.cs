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
    public void OnClicBackRoadTouchUI()
    {
        //UIManager.Instance.IsStop = !UIManager.Instance.IsStop;
        //Time.timeScale = UIManager.Instance.IsStop ? 0f : 1f;
        _uiBuildPanel.SetActive(true);
        _roadTouchUIPanel.SetActive(false);
    }

}
