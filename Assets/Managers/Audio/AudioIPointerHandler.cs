using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AudioIPointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        print("mouse has entered");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        print("mouse has clicked");
    }
}
