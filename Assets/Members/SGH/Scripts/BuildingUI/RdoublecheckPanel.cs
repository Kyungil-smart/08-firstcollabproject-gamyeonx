using UnityEngine;

public class RdoublecheckPanel : MonoBehaviour
{
    [SerializeField] private GameObject _roadDemolish;
    [SerializeField] private GameObject _rDoublecheck;

    public void OnClickSetRdoublecheck()
    {
        //_roadDemolish.SetActive(false);
        //if (GridBuildingSystem.Instance._temp != null)
        //{
        //    GridBuildingSystem.Instance._temp.IsMenuOpen = false;
        //    GridBuildingSystem.Instance._temp = null;
        //}
        _rDoublecheck.SetActive(true);
    }

    public void OnClickNoRdoublecheck()
    {
        _roadDemolish.SetActive(true);
        _rDoublecheck.SetActive(false);
    }

    public void OnClickCloseRdoublecheck()
    {
        _rDoublecheck.SetActive(false);
        _roadDemolish.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null)
        {
            GridBuildingSystem.Instance._temp.IsMenuOpen = false;
            GridBuildingSystem.Instance._temp = null;
        }
    }
}
