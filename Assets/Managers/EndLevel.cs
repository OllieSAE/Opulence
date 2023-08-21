using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevel : MonoBehaviour
{
    public bool levelEnded = false;
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
                if (levelEnded == false)
                {
                    levelEnded = true;
                    print("test");
                
                    //maybe replace with coroutine so we can animate the exit
                    GameManager.Instance.EndLevel();
                }
            }
            else if (!bossDoor)
            {
                if (levelEnded == false)
                {
                    levelEnded = true;
                    print("test");
                
                    //maybe replace with coroutine so we can animate the exit
                    GameManager.Instance.EndLevel();
                }
            }
            
        }
    }
}
