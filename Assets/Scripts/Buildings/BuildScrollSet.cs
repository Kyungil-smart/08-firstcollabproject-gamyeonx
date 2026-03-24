using UnityEngine;
using UnityEngine.UI;

public class BuildScrollSet : MonoBehaviour
{
    [SerializeField] private GameObject _roadScroll;
    [SerializeField] private GameObject _buildScroll;

    public void OnClickForBuildPanel()
    {
        _roadScroll.SetActive(false);
        _buildScroll.SetActive(true);
    }
}
