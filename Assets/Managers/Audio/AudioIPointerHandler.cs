using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class AudioIPointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public string hoverSoundToPlay;
    public string clickSoundToPlay;
    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSoundToPlay != null)
        {
            RuntimeManager.PlayOneShot("event:/SOUND EVENTS/" + hoverSoundToPlay);
        }
    }
    

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSoundToPlay != null)
        {
            RuntimeManager.PlayOneShot("event:/SOUND EVENTS/" + clickSoundToPlay);
        }
    }
}
