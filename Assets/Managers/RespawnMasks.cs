using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RespawnMasks : MonoBehaviour
{
    public GameObject mask1, mask2, mask3, mask4;

    public Button respawnMasks;

    public void Start()
    {
        RespawnAllMasks();
        respawnMasks.gameObject.SetActive(false);
    }

    public void RespawnAllMasks()
    {
        mask1.SetActive(true);
        mask2.SetActive(true);
        mask3.SetActive(true);
        mask4.SetActive(true);
    }

    private void Update()
    {
        if (!mask1.activeSelf && !mask2.activeSelf && !mask3.activeSelf && !mask4.activeSelf)
        {
            respawnMasks.gameObject.SetActive(true);
        }
        else respawnMasks.gameObject.SetActive(false);
    }
}
