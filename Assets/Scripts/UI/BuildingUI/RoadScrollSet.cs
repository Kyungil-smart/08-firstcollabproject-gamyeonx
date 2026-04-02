using UnityEngine;
using UnityEngine.UI;

public class RoadScrollSet : MonoBehaviour
{
    [SerializeField] private GameObject _roadScroll;
    [SerializeField] private GameObject _buildScroll;

    public void OnClickForRoadPanel()
    {
        _buildScroll.SetActive(false);
        _roadScroll.SetActive(true);
    }
}