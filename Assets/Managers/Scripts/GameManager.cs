using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class GameManager : MonoBehaviour
{
    
    public GameObject player;
    public Vector3 playerRespawnPos;
    public bool isPaused;
    public GameObject pauseUI;
    private AsyncOperation loadingOperation;
    public GameObject loadingScreen;
    
    [Header("Tutorial Stuff")]
    public bool tutorialTestEnable;
    public bool combatTutorialTestEnable;
    public bool toggleEnemyMovement;
    public bool mainMenuEnabled;
    public ObjectPickUpTest objectPickUpTest;
    public GameObject mainMenuUI;
    private string sceneToLoad;
    private string currentScene;
    public GameObject tutorialStartUI;
    public GameObject tutorialEndUI;
    public GameObject startCombatUI;
    public GameObject combatUI;
    public GameObject finalCombatUI;
    public GameObject creditsUI;
    public GameObject controlsUI;
    public ZhiaHeadCheck zhiaSkeleton;
    public bool testObjectPickedUp;
    public GameObject firstFloor;
    public GameObject secondFloor;
    public GameObject mainCamera;
    
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
    #region Event Delegates

    public delegate void OnLevelLoadedEvent();
    public event OnLevelLoadedEvent onLevelLoadedEvent;
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
    #endregion

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        testObjectPickedUp = false;
        _instance = this;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        tutorialStartUI.SetActive(false);
        tutorialEndUI.SetActive(false);
        pauseUI.SetActive(false);
        startCombatUI.SetActive(false);
        creditsUI.SetActive(false);
        if (!mainMenuEnabled) mainMenuUI.SetActive(false);
        else mainMenuUI.SetActive(true);
        loadingScreen.SetActive(false);
    }

    private void Start()
    {
        OnLevelLoaded();
    }

    #region Level Select/Load

    public void SelectMovementLevel()
    {
        sceneToLoad = "MovementTestScene";
        tutorialTestEnable = true;
        combatTutorialTestEnable = false;
        StartCoroutine(LoadLevelCoroutine());
    }

    public void SelectCombatLevel()
    {
        sceneToLoad = "CombatTestScene";
        tutorialTestEnable = false;
        combatTutorialTestEnable = true;
        StartCoroutine(LoadLevelCoroutine());
    }

    public void SelectAudioLevel()
    {
        sceneToLoad = "AudioPedestalScene 1";
        tutorialTestEnable = false;
        combatTutorialTestEnable = false;
        StartCoroutine(LoadLevelCoroutine());
    }

    public void SelectActualLevel()
    {
        sceneToLoad = "LevelGenTest";
        tutorialTestEnable = false;
        combatTutorialTestEnable = false;
        StartCoroutine(LoadLevelCoroutine());
    }

    public void SelectCreditsUI()
    {
        mainMenuUI.SetActive(false);
        creditsUI.SetActive(true);
    }

    public void SelectControlsUI()
    {
        mainMenuUI.SetActive(false);
        controlsUI.SetActive(true);
    }

    public void ExitCreditsOrControls()
    {
        creditsUI.SetActive(false);
        controlsUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }
    

    private IEnumerator LoadLevelCoroutine()
    {
        mainMenuUI.SetActive(false);
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(1f);
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad,LoadSceneMode.Additive);
        while (!loadingOperation.isDone)
        {
            yield return null;
        }

        if (sceneToLoad != "LevelGenTest")
        {
            OnLevelLoaded();
            loadingScreen.SetActive(false);
        }
    }

    public void LevelGenComplete()
    {
        OnLevelLoaded();
        loadingScreen.SetActive(false);
    }

    #endregion
    
    private void OnLevelLoaded()
    {
        currentScene = sceneToLoad;
        player = GameObject.FindGameObjectWithTag("Player");
        if( player != null ) player.SetActive(true);
        mainCamera.GetComponent<StudioListener>().attenuationObject = player;
        objectPickUpTest = FindObjectOfType<ObjectPickUpTest>();
        zhiaSkeleton = FindObjectOfType<ZhiaHeadCheck>();
        firstFloor = GameObject.FindGameObjectWithTag("First Floor");
        secondFloor = GameObject.FindGameObjectWithTag("Second Floor");
        if(objectPickUpTest != null) objectPickUpTest.ObjectPickUp += ObjectPickedUp;
        if(player != null) playerRespawnPos = player.transform.position;
        if (combatTutorialTestEnable)
        {
            startCombatUI.SetActive(true);
            player.GetComponent<Combat>().enabled = true;
            CombatTestManager.Instance.combatUI = combatUI;
            CombatTestManager.Instance.finalCombatUI = tutorialEndUI;
        }
        else if (player != null) player.GetComponent<Combat>().enabled = false;
        onLevelLoadedEvent?.Invoke();
        enableEnemyPatrolEvent?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //like a wild chicken, this is FOUL
            if (mainMenuUI.activeSelf || tutorialStartUI.activeSelf || tutorialEndUI.activeSelf ||
                startCombatUI.activeSelf || combatUI.activeSelf || finalCombatUI.activeSelf || creditsUI.activeSelf ||
                controlsUI.activeSelf) return;
                PauseUI();
        }
    }

    public void SubscribeToDeathEvents(Health health)
    {
        health.deathEvent += SomethingDied;
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

        if (combatTutorialTestEnable)
        {
            startCombatUI.SetActive(false);
            enableEnemyPatrolEvent?.Invoke();
        }
    }

    private void ObjectPickedUp()
    {
        testObjectPickedUp = true;
        if(zhiaSkeleton!=null) zhiaSkeleton.playerHasObject = true;
    }

    private void SomethingDied(GameObject deadThing)
    {
        if (deadThing.layer == 6)
        {
            RespawnPlayer();
            if(objectPickUpTest!=null)RespawnObject();
        }
        else if (deadThing.layer == 8)
        {
            deadThing.GetComponent<DamageAOETest>().enabled = false;
            deadThing.GetComponent<BasicEnemyPatrol>().enabled = false;
        }
    }

    public void DestroyFirstFloor()
    {
        if (firstFloor != null) Destroy(firstFloor);
    }

    public void DestroySecondFloor()
    {
        if (secondFloor != null) Destroy(secondFloor);
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

    public void ExitLevel()
    {
        if (currentScene != "LevelSelectScene")
        {
            if(isPaused) PauseUI();
            player.SetActive(false);
            player.GetComponent<Movement>().playerWalk.stop(STOP_MODE.ALLOWFADEOUT);
            tutorialEndUI.SetActive(false);
            SceneManager.UnloadSceneAsync(currentScene);
            mainMenuUI.SetActive(true);
        }
    }

    public void ExitProgram()
    {
        Application.Quit();
    }
    
    //Testing only!!!
    public void SlowTime()
    {
        Time.timeScale = 0.1f;
    }

    public void DefaultTime()
    {
        Time.timeScale = 1f;
    }
}
