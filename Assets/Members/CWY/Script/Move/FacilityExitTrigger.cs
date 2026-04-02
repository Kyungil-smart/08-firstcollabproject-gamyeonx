using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FacilityExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        GuestController guest = other.GetComponent<GuestController>();

        if (guest == null)
        {
            return;
        }

        guest.HandleReachedFacilityExitTrigger();
    }
}