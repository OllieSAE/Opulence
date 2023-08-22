using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ControlsUIToggler : MonoBehaviour
{
    public List<GameObject> pcControls, xboxControls, ps45Controls, switchControls;

    private List<GameObject> currentList;
    private enum ControlsType
    {
        PC,
        XBOX,
        PS45,
        SWITCH
    }
    private ControlsType currentControls;
    
    private void OnEnable()
    {
        DisableAllControlsUI();
    }

    private void DisableAllControlsUI()
    {
        foreach (GameObject go in pcControls)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in xboxControls)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in ps45Controls)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in switchControls)
        {
            go.SetActive(false);
        }
    }


    public void EnablePCControlsUI()
    {
        DisableAllControlsUI();
        ActivateList(ControlsType.PC);
    }

    public void EnableXboxControlsUI()
    {
        DisableAllControlsUI();
        ActivateList(ControlsType.XBOX);
    }

    public void EnablePS45ControlsUI()
    {
        DisableAllControlsUI();
        ActivateList(ControlsType.PS45);
    }

    public void EnableSwitchControlsUI()
    {
        DisableAllControlsUI();
        ActivateList(ControlsType.SWITCH);
    }

    private void ActivateList(ControlsType type)
    {
        switch (type)
        {
            case ControlsType.PC:
                ShowControlsUI(pcControls);
                break;
            case ControlsType.XBOX:
                ShowControlsUI(xboxControls);
                break;
            case ControlsType.PS45:
                ShowControlsUI(ps45Controls);
                break;
            case ControlsType.SWITCH:
                ShowControlsUI(switchControls);
                break;
        }
    }

    public void ShowControlsUI(List<GameObject> listToShow)
    {
        foreach (GameObject go in listToShow)
        {
            go.SetActive(true);
        }
    }
}
