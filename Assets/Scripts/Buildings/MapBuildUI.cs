using UnityEngine;
using UnityEngine.UI;

public class MapBuildUI : MonoBehaviour
{
    [SerializeField] private GameObject _roadScroll;
    [SerializeField] private GameObject _buildScroll;

    void Start()
    {
        gameObject.SetActive(false);
        _buildScroll.SetActive(false);
    }
}
