using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverSound;
    public AudioClip clickSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && SoundManager.instance != null)
            SoundManager.instance?.PlayMenuHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && SoundManager.instance != null)
            SoundManager.instance?.PlayMenuClick();
    }
}
