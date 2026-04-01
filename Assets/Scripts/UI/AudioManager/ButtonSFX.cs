using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayButtonHoverSFX();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayButtonPressSFX();
    }
}