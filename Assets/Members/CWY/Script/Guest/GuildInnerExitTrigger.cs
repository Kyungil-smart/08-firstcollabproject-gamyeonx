using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GuildInnerExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        GuestController guest = other.GetComponent<GuestController>();

        if (guest == null)
        {
            return;
        }

        if (guest.ExitFlowHandler == null)
        {
            return;
        }

        guest.ExitFlowHandler.NotifyEnteredGuildInnerExitTrigger();
    }
}