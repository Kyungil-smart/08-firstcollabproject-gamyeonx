using UnityEngine;

public class RestaurantUI : MonoBehaviour
{
    [SerializeField] private GameObject _restaurantUI;

    public void OnclickBack()
    {
        _restaurantUI.SetActive(false);
    }
}
