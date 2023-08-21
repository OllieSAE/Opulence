using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoorOpener : MonoBehaviour
{
    public GameObject exitDoor;
    public Sprite spriteToChange;
    private SpriteRenderer spriteRenderer;
    private bool bossDead = false;
    public bool bossDoor;

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
            }
            else if (!bossDoor)
            {
                spriteRenderer = exitDoor.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = spriteToChange;
            }
        }
    }
}
