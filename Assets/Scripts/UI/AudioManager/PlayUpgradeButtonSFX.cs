using UnityEngine;
using UnityEngine.EventSystems;

public class PlayUpgradeButtonSFX : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayUpgradeButtonSFX();
    }
}
