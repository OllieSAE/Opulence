using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class ExitDoorOpener : MonoBehaviour
{
    public GameObject exitDoor;
    public Sprite spriteToChange;
    private SpriteRenderer spriteRenderer;
    private bool bossDead = false;
    public bool bossDoor;
    public bool startDoor;
    private bool soundPlayed = false;

    private void Start()
    {
        GameManager.Instance.bossKilledEvent += BossKilled;
    }

    private void OnDisable()
    {
        GameManager.Instance.bossKilledEvent -= BossKilled;
    }

    private void BossKilled()
    {
        bossDead = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (bossDoor && bossDead)
            {
                spriteRenderer = exitDoor.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = spriteToChange;
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Door Open");
                soundPlayed = true;
            }
            else if (!bossDoor)
            {
                spriteRenderer = exitDoor.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = spriteToChange;
            }

            if (startDoor && !soundPlayed && !bossDoor)
            {
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Door Close");
                soundPlayed = true;
            }
            else if (!startDoor && !soundPlayed && !bossDoor)
            {
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Door Open");
                soundPlayed = true;
            }
        }
    }
}
