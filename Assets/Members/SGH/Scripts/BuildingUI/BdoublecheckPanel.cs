using UnityEngine;

public class BdoublecheckPanel : MonoBehaviour
{
    [SerializeField] private GameObject _buildDemolish;
    [SerializeField] private GameObject _bDoublecheck;

    public void OnClickSetDdoublecheck()
    {
        //_buildDemolish.SetActive(false);
        //if (GridBuildingSystem.Instance._temp != null)
        //{
        //    GridBuildingSystem.Instance._temp.IsMenuOpen = false;
        //    GridBuildingSystem.Instance._temp = null;
        //}
        _bDoublecheck.SetActive(true);
    }

    public void OnClickNoBdoublecheck()
    {
        _buildDemolish.SetActive(true);
        _bDoublecheck.SetActive(false);
    }
    public void OnClickCloseBdoublecheck()
    {
        _bDoublecheck.SetActive(false);
        _buildDemolish.SetActive(false);
        if (GridBuildingSystem.Instance._temp != null)
        {
            GridBuildingSystem.Instance._temp.IsMenuOpen = false;
            GridBuildingSystem.Instance._temp = null;
        }
    }

    public void OnClickDemolishYes()
    {
        GridBuildingSystem.Instance.DeleteSelectedBuilding();
    }
}
