using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public ObjectPickUpTest objectPickUpTest;
    public GameObject player;
    public Transform playerRespawnPos;
    public bool testObjectPickedUp;
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                print("Game Manager is null!");
            }

            return _instance;
        }
    }
    
    public delegate void PlayerRespawnEvent();
    public event PlayerRespawnEvent playerRespawnEvent;
    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        testObjectPickedUp = false;
        _instance = this;
    }

    //this will break when GM exists before level loaded
    private void OnEnable()
    {
        objectPickUpTest.ObjectPickUp += ObjectPickedUp;
        player.GetComponent<Health>().deathEvent += SomethingDied;
        
        //TODO:
        //subscribe to other Health's death events when they're created
        
        playerRespawnPos = player.transform;
    }

    private void ObjectPickedUp()
    {
        testObjectPickedUp = true;
    }

    private void SomethingDied(GameObject deadThing)
    {
        if (deadThing.layer == 6)
        {
            RespawnPlayer();
            print(deadThing.ToString() + " has died!");
        }
        else
        {
            //print(deadThing.ToString() + " has died!");
        }
    }

    private void RespawnPlayer()
    {
        player.transform.position = playerRespawnPos.position;
        playerRespawnEvent?.Invoke();
        print("respawn event invoked from GM");
    }
    
    
}
