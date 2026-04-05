using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteButtonSFX : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayDeleteButtonSFX();
    }
}
