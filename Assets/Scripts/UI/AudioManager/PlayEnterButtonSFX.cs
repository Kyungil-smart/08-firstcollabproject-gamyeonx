using UnityEngine;
using UnityEngine.EventSystems;

public class PlayEnterButtonSFX : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayEnterButtonSFX();
    }
}
