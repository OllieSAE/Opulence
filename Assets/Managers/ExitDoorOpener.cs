using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoorOpener : MonoBehaviour
{
    public GameObject exitDoor;
    public Sprite spriteToChange;
    private SpriteRenderer spriteRenderer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            spriteRenderer = exitDoor.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = spriteToChange;
        }
    }
}
