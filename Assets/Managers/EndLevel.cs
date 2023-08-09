using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevel : MonoBehaviour
{
    public bool levelEnded = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
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
