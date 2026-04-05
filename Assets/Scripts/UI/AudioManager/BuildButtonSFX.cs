using UnityEngine;
using UnityEngine.EventSystems;

public class BuildButtonSFX : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayBuildButtonSFX();
    }
}
