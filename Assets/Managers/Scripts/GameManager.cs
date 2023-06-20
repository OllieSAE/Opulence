using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public GameObject player;
    public Vector3 playerRespawnPos;
    public bool isPaused;
    public GameObject pauseUI;
    
    [Header("Tutorial Stuff")]
    public bool tutorialTestEnable;
    public bool toggleEnemyMovement;
    public ObjectPickUpTest objectPickUpTest;
    public GameObject tutorialStartUI;
    public GameObject tutorialEndUI;
    public ZhiaHeadCheck zhiaSkeleton;
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

    public delegate void TutorialDialogueFinishedEvent();
    public event TutorialDialogueFinishedEvent tutorialDialogueFinishedEvent;

    public delegate void EndTutorialEvent();
    public event EndTutorialEvent endTutorialEvent;

    public delegate void PauseStartEvent();

    public event PauseStartEvent pauseStartEvent;
    
    public delegate void PauseEndEvent();

    public event PauseEndEvent pauseEndEvent;

    public delegate void EnableEnemyPatrolEvent();

    public event EnableEnemyPatrolEvent enableEnemyPatrolEvent;
    
    public delegate void DisableEnemyPatrolEvent();

    public event DisableEnemyPatrolEvent disableEnemyPatrolEvent;
    

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        testObjectPickedUp = false;
        _instance = this;
        tutorialStartUI.SetActive(false);
        tutorialEndUI.SetActive(false);
        pauseUI.SetActive(false);
    }

    //this will break when GM exists before level loaded
    private void OnEnable()
    {
        objectPickUpTest.ObjectPickUp += ObjectPickedUp;
        player.GetComponent<Health>().deathEvent += SomethingDied;
        
        //TODO:
        //subscribe to other Health's death events when they're created
        
        playerRespawnPos = player.transform.position;
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUI();
        }
    }

    public void ToggleEnemyMovement()
    {
        toggleEnemyMovement = !toggleEnemyMovement;
        if (toggleEnemyMovement)
        {
            enableEnemyPatrolEvent?.Invoke();
        }

        if (!toggleEnemyMovement)
        {
            disableEnemyPatrolEvent?.Invoke();
        }
    }

    private void ObjectPickedUp()
    {
        testObjectPickedUp = true;
        zhiaSkeleton.playerHasObject = true;
    }

    private void SomethingDied(GameObject deadThing)
    {
        if (deadThing.layer == 6)
        {
            RespawnPlayer();
            RespawnObject();
            //print(deadThing.ToString() + " has died!");
        }
        else
        {
            //print(deadThing.ToString() + " has died!");
        }
    }

    private void RespawnPlayer()
    {
        playerRespawnEvent?.Invoke();
    }

    private void RespawnObject()
    {
        objectPickUpTest.gameObject.SetActive(true);
        zhiaSkeleton.playerHasObject = false;
    }

    public void DialogueManagerEvent()
    {
        tutorialStartUI.SetActive(true);
    }

    public void BeginTutorial()
    {
        tutorialStartUI.SetActive(false);
        tutorialDialogueFinishedEvent?.Invoke();
    }

    public void EndTutorial()
    {
        tutorialEndUI.SetActive(true);
        endTutorialEvent?.Invoke();
    }

    public void PauseUI()
    {
        if (isPaused)
        {
            pauseUI.SetActive(false);
            pauseEndEvent?.Invoke();
        }
        else if (!isPaused)
        {
            pauseUI.SetActive(true);
            pauseStartEvent?.Invoke();
        }

        isPaused = !isPaused;

    }

    public void ExitProgram()
    {
        Application.Quit();
    }
}
