using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonTextManager : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
{
    public Button button;
    public bool backButton;

    private TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        if (backButton)
        {
            text.color = button.colors.normalColor;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (!backButton)
        {
            if (button)
            {
                if (button.interactable)
                {
                    text.color = button.colors.normalColor;
                }
                else
                {
                    text.color = button.colors.disabledColor;

                }
            }
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backButton)
        {
            text.color = button.colors.highlightedColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (backButton)
        {
            text.color = button.colors.pressedColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (backButton)
        {
            if (text.color != button.colors.pressedColor)
            {
                text.color = button.colors.normalColor;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (backButton)
        {
            text.color = button.colors.selectedColor;
        }
    }
}
