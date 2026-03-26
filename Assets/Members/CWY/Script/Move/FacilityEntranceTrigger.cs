using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FacilityEntranceTrigger : MonoBehaviour
{
    [SerializeField] private FacilityRuntime _facilityRuntime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GuestController guest = other.GetComponent<GuestController>();

        if (guest == null)
        {
            return;
        }

        if (_facilityRuntime == null)
        {
            Debug.Log("[FacilityEntranceTrigger] FacilityRuntime이 연결되지 않았습니다.");
            return;
        }

        if (guest.CurrentTargetFacilityID != _facilityRuntime.FacilityID)
        {
            return;
        }

        guest.EnterFacility(_facilityRuntime);
    }
}