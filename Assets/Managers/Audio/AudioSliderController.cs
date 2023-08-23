using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioSliderController : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slider != null) GameManager.Instance.currentSlider = slider;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.currentSlider = null;
    }
}
