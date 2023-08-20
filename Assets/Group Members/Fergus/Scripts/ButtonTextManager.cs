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
    private Image buttonImage;
    
    void Start()
    {
        // Check if the textMesh is already saved
        if (text == null)
        {
            // If there is no text currently, set the text
            text = GetComponent<TextMeshProUGUI>();
        }
        // Checking if we are a back button
        if (backButton)
        {
            // Setting the text's colour to the buttons normal colour
            text.color = button.colors.normalColor;
            // Getting the sprite currently on the button
            buttonImage = button.GetComponent<Image>();
        }
    }

    void Update()
    {
        // Checking we aren't a backbutton
        if (!backButton)
        {
            // Checking we have a button available
            if (button)
            {
                // Checking if we are interactable or not
                if (button.interactable)
                {
                    // Setting text colour to buttons normal colour
                    text.color = button.colors.normalColor;
                }
                else
                {
                    // Setting text colour to buttons disabled colour
                    text.color = button.colors.disabledColor;
                }
            }
        }

    }

    // Event function for when the mouse enters the script objects space
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backButton)
        {
            text.color = button.colors.highlightedColor;
        }
    }

    // Event function for when the mouse clicks down on the script objects space
    public void OnPointerDown(PointerEventData eventData)
    {
        if (backButton)
        {
            text.color = button.colors.pressedColor;
            buttonImage.color = button.colors.pressedColor;
        }
    }

    // Event function for when the mouse exits the script objects space
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

    // Event function for when the mouse releases a click on the script objects space
    public void OnPointerUp(PointerEventData eventData)
    {
        if (backButton)
        {
            text.color = button.colors.selectedColor;
            buttonImage.color = Color.white;
        }
    }
}
