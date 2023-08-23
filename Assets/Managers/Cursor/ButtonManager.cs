using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public List<Button> mainMenuButtonList;
    public List<Button> testButtonList;
    public int currentButton;
    //original highlighted hexcode = C1C1C1

    private void Start()
    {
        //buttonList[0].image.color = buttonList[0].colors.highlightedColor;
        mainMenuButtonList[0].image.color = Color.white;
        currentButton = 0;
        StartCoroutine(StartPopulateCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentButton < testButtonList.Count - 1)
            {
                testButtonList[currentButton].image.color = testButtonList[currentButton].colors.normalColor;
                testButtonList[currentButton + 1].image.color = testButtonList[currentButton + 1].colors.highlightedColor;
                currentButton++;
            }
            else
            {
                testButtonList[currentButton].image.color = testButtonList[currentButton].colors.normalColor;
                testButtonList[0].image.color = testButtonList[0].colors.highlightedColor;
                currentButton = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            testButtonList[currentButton].Select();
            StartCoroutine(PopulateActiveButtonsCoroutine());
        }
    }

    private IEnumerator StartPopulateCoroutine()
    {
        testButtonList.Clear();
        yield return new WaitForSeconds(0.1f);
        testButtonList.AddRange(FindObjectsOfType<Button>());
        currentButton = 0;
        testButtonList.Reverse();
        testButtonList[0].image.color = testButtonList[0].colors.highlightedColor;
    }

    private IEnumerator PopulateActiveButtonsCoroutine()
    {
        yield return new WaitForSeconds(0.05f);
        if (testButtonList[currentButton].isActiveAndEnabled)
        {
            print("hi");
        }
        else
        {
            testButtonList.Clear();
            yield return new WaitForSeconds(0.1f);
            testButtonList.AddRange(FindObjectsOfType<Button>());
            currentButton = 0;
            testButtonList.Reverse();
            testButtonList[0].image.color = testButtonList[0].colors.highlightedColor;
        }
    }
}
