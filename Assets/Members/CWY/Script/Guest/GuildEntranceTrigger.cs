using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GuildEntranceTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        GuestController guest = other.GetComponent<GuestController>();

        if (guest == null)
        {
            return;
        }

        if (guest.EntryFlowHandler == null)
        {
            return;
        }

        guest.EntryFlowHandler.NotifyEnteredGuildEntranceTrigger();
    }
}