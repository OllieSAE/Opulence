using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FMODUnity;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class GameManager : MonoBehaviour
{
    public StatTracker statTracker;
    public GameObject player;
    public Canvas playerCanvas;
    public Vector3 playerRespawnPos;
    public bool isPaused;
    public bool mapIsOpen;
    private AsyncOperation loadingOperation;
    public GameObject mainCamera;
    public Volume globalVolume;
    public Volume globalVolumeWithDoF;
    public GameObject vcam1;
    public GameObject mapCamera;
    public CinemachineBlendDefinition goToMapCameraBlend;
    private CinemachineVirtualCamera vcam1VC;
    private CinemachineVirtualCamera mapCameraVC;
    private CinemachineBrain mainCameraBrain;
    public string sceneToLoad;
    public string currentScene;
    public GameObject playerMapClone;
    private int playerHealthEndOfLevel;

    [Header("Backgrounds")]
    public GameObject mainMenuBG;
    public GameObject creditsBG;
    public GameObject playOptionsBG;
    public GameObject loadingScreenBG;
    public GameObject settingsBG;
    public GameObject loadGameMenuBG;
    public GameObject statsBG;
    
    [Header("UI Elements")]
    public GameObject mainMenuUI;
    public GameObject creditsUI;
    public GameObject loadingScreen;
    public GameObject playMenuUI;
    public GameObject settingsMenuUI;
    public GameObject loadGameMenuUI;
    public GameObject pauseUI;
    public GameObject statsUI;
    public Button stuckButton;
    
    
    //public GameObject controlsUI;
    
    [Header("Tutorial Stuff")]
    public bool toggleEnemyMovement;
    public bool mainMenuEnabled;
    //public bool tutorialTestEnable;
    //public bool combatTutorialTestEnable;
    //public ObjectPickUpTest objectPickUpTest;
    //public GameObject tutorialStartUI;
    //public GameObject tutorialEndUI;
    //public GameObject startCombatUI;
    //public GameObject combatUI;
    //public GameObject finalCombatUI;
    //public ZhiaHeadCheck zhiaSkeleton;
    //public bool testObjectPickedUp;
    //public GameObject firstFloor;
    //public GameObject secondFloor;
    
    
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

    public delegate void MapOpenedEvent();
    public event MapOpenedEvent mapOpenedEvent;

    public delegate void MapClosedEvent();
    public event MapClosedEvent mapClosedEvent;
    #endregion

    #region FMOD Event Instances
    public FMOD.Studio.EventInstance mainMenuMusic;
    public FMOD.Studio.EventInstance ambienceMusic;
    public FMOD.Studio.EventInstance levelMusic;
    public FMOD.Studio.EventInstance bossMusic;
    #endregion
    
    
    private void Awake()
    {
        //figure out saving stuff
        //make sure to save audio and other settings too
        
        DontDestroyOnLoad(this.gameObject);
        //testObjectPickedUp = false;
        _instance = this;
        statTracker = GetComponent<StatTracker>();
        statTracker.ResetAllValues();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null) mainCameraBrain = mainCamera.GetComponent<CinemachineBrain>();
        vcam1 = GameObject.FindGameObjectWithTag("vcam1");
        if (vcam1 != null) vcam1VC = vcam1.GetComponent<CinemachineVirtualCamera>();
        mapCamera = GameObject.FindGameObjectWithTag("Map Camera");
        if (mapCamera != null) mapCameraVC = mapCamera.GetComponent<CinemachineVirtualCamera>();
        mainCameraBrain.m_DefaultBlend = goToMapCameraBlend;
        //tutorialStartUI.SetActive(false);
        //tutorialEndUI.SetActive(false);
        //pauseUI.SetActive(false);
        //startCombatUI.SetActive(false);
        //creditsUI.SetActive(false);
        if (!mainMenuEnabled)
        {
            mainMenuBG.SetActive(false);
            mainMenuUI.SetActive(false);
        }
        else
        {
            mainMenuBG.SetActive(true);
            mainMenuUI.SetActive(true);
        }
        //loadingScreen.SetActive(false);
        sceneToLoad = "LevelSelectScene";


        mainMenuMusic = RuntimeManager.CreateInstance("event:/SOUND EVENTS/Music Menu");
        ambienceMusic = RuntimeManager.CreateInstance("event:/SOUND EVENTS/Ambience");
        levelMusic = RuntimeManager.CreateInstance("event:/SOUND EVENTS/Music Level");
        bossMusic = RuntimeManager.CreateInstance("event:/SOUND EVENTS/Music Boss");
        RuntimeManager.AttachInstanceToGameObject(mainMenuMusic, transform, true);
        RuntimeManager.AttachInstanceToGameObject(ambienceMusic, transform,true);
        RuntimeManager.AttachInstanceToGameObject(levelMusic, transform, true);
        RuntimeManager.AttachInstanceToGameObject(bossMusic, transform,true);

        if (!FmodExtensions.IsPlaying(mainMenuMusic))
        {
            mainMenuMusic.start();
        }
        if (!FmodExtensions.IsPlaying(ambienceMusic))
        {
            ambienceMusic.start();
        }
    }

    private void Start()
    {
        OnLevelLoaded();
    }

    private void OnDisable()
    {
        mainMenuMusic.release();
        ambienceMusic.release();
        levelMusic.release();
        bossMusic.release();
    }

    #region Level Select/Load

    public void SelectMovementLevel()
    {
        // sceneToLoad = "MovementTestScene";
        // tutorialTestEnable = true;
        // combatTutorialTestEnable = false;
        // StartCoroutine(LoadLevelCoroutine());
    }

    public void SelectCombatLevel()
    {
        // sceneToLoad = "CombatTestScene";
        // tutorialTestEnable = false;
        // combatTutorialTestEnable = true;
        // StartCoroutine(LoadLevelCoroutine());
    }

    public void SelectAudioLevel()
    {
        // sceneToLoad = "AudioPedestalScene 1";
        // tutorialTestEnable = false;
        // combatTutorialTestEnable = false;
        // StartCoroutine(LoadLevelCoroutine());
    }

    public void SelectActualLevel()
    {
        //need to change this to opening cutscene
        sceneToLoad = "OpeningCutscene";
        // tutorialTestEnable = false;
        // combatTutorialTestEnable = false;
        StartCoroutine(LoadLevelCoroutine());
    }

    public void SelectCreditsUI()
    {
        mainMenuUI.SetActive(false);
        creditsUI.SetActive(true);
    }

    public void SelectControlsUI()
    {
        // mainMenuUI.SetActive(false);
        // controlsUI.SetActive(true);
    }

    public void ExitCreditsOrControls()
    {
        // creditsUI.SetActive(false);
        // controlsUI.SetActive(false);
        // mainMenuUI.SetActive(true);
    }

    public void MainMenuPlayButton()
    {
        mainMenuBG.SetActive(false);
        mainMenuUI.SetActive(false);
        
        playOptionsBG.SetActive(true);
        playMenuUI.SetActive(true);
    }

    public void SettingsButton()
    {
        mainMenuBG.SetActive(false);
        mainMenuUI.SetActive(false);
        pauseUI.SetActive(false);
        
        settingsBG.SetActive(true);
        settingsMenuUI.SetActive(true);
    }

    public void CreditsButton()
    {
        mainMenuBG.SetActive(false);
        mainMenuUI.SetActive(false);

        creditsBG.SetActive(true);
        creditsUI.SetActive(true);
    }

    public void NewGameButton()
    {
        playOptionsBG.SetActive(false);
        playMenuUI.SetActive(false);
        
        SelectActualLevel();
    }

    public void LoadGameButton()
    {
        playOptionsBG.SetActive(false);
        playMenuUI.SetActive(false);

        loadGameMenuBG.SetActive(true);
        loadGameMenuUI.SetActive(true);
    }

    public void LoadSaveSlot1()
    {
        //TODO:
        //something like fileToLoad = slot 1 path
        //LoadSavedGameButton(fileToLoad)
    }
    
    public void LoadSaveSlot2()
    {
        //TODO:
        //something like fileToLoad = slot 2 path
        //LoadSavedGameButton(fileToLoad)
    }
    
    public void LoadSaveSlot3()
    {
        //TODO:
        //something like fileToLoad = slot 3 path
        //LoadSavedGameButton(fileToLoad)
    }
    
    public void LoadSavedGameButton()
    {
        //TODO:
    }

    public void BackToMainMenu()
    {
        playOptionsBG.SetActive(false);
        playMenuUI.SetActive(false);
        settingsBG.SetActive(false);
        settingsMenuUI.SetActive(false);
        creditsBG.SetActive(false);
        creditsUI.SetActive(false);
        statsUI.SetActive(false);
        statsBG.SetActive(false);
        mainMenuBG.SetActive(true);
        mainMenuUI.SetActive(true);
    }

    public void BackToPlayOptionsMenu()
    {
        loadGameMenuBG.SetActive(false);
        loadGameMenuUI.SetActive(false);
        
        playOptionsBG.SetActive(true);
        playMenuUI.SetActive(true);
    }

    public void BackToPauseUI()
    {
        settingsBG.SetActive(false);
        settingsMenuUI.SetActive(false);

        pauseUI.SetActive(true);
    }

    public void BackFromSettings()
    {
        if (currentScene != "LevelSelectScene")
        {
            BackToPauseUI();
        }
        else BackToMainMenu();
    }

    public void RescuePlayer()
    {
        if (player != null && currentScene == "LevelGenTest")
        {
            PauseUI();
            player.GetComponentInChildren<RescueFailsafe>().FailsafeRescuePlayer();
        }
    }
    

    private IEnumerator LoadLevelCoroutine()
    {
        mainMenuBG.SetActive(false);
        mainMenuUI.SetActive(false);
        
        if(vcam1!=null)vcam1.transform.position = new Vector3(0, 0, -20);
        loadingScreenBG.SetActive(true);
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
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
        
        if(vcam1!=null)vcam1.transform.position = new Vector3(0, 0, -20);
        //if (mapCameraVC != null) mapCameraVC.transform.position = new Vector3(0, 0, -20);

        if (sceneToLoad != "LevelGenTest" && sceneToLoad != "FirstBossLevel")
        {
            OnLevelLoaded();
            loadingScreenBG.SetActive(false);
            loadingScreen.SetActive(false);
        }
        else if (sceneToLoad == "FirstBossLevel")
        {
            OnLevelLoaded();
            StartCoroutine(LoadingScreenDeactivate());
        }
    }

    //TODO:
    //HACK: Hides the view of the VFX playing on awake in boss and first level scenes
    public IEnumerator LoadingScreenDeactivate()
    {
        yield return new WaitForSeconds(0f);
        loadingScreenBG.SetActive(false);
        loadingScreen.SetActive(false);
    }

    public void LevelGenComplete()
    {
        OnLevelLoaded();
        statTracker.UpdateTimeElapsed(Time.time,0);
        StartCoroutine(LoadingScreenDeactivate());
    }

    #endregion
    
    private void OnLevelLoaded()
    {
        currentScene = sceneToLoad;
        if (!string.IsNullOrEmpty(currentScene))
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentScene));
        }
        if(LevelGenerator.Instance != null) LevelGenerator.Instance.SpawnEnemies();
        player = GameObject.FindGameObjectWithTag("Player");
        
        //HACK CITY
        //to make the player health bar NOT visible during level gen
        if (player!=null) playerCanvas = player.GetComponentInChildren<Canvas>();
        if (playerCanvas != null) playerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        
        if (player != null)
        {
            player.SetActive(true);
            mainCamera.GetComponent<StudioListener>().attenuationObject = player;
            
        }
        
        if (player != null)
        {
            player.GetComponent<Combat>().enabled = true;
            playerRespawnPos = player.transform.position;
            StartCoroutine(UnlockPlayer());
            //player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            //player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (currentScene == "LevelGenTest")
        {
            vcam1.GetComponent<CinemachineVirtualCamera>().m_Follow = player.transform;
        }
        // objectPickUpTest = FindObjectOfType<ObjectPickUpTest>();
        // zhiaSkeleton = FindObjectOfType<ZhiaHeadCheck>();
        // firstFloor = GameObject.FindGameObjectWithTag("First Floor");
        // secondFloor = GameObject.FindGameObjectWithTag("Second Floor");
        // if(objectPickUpTest != null) objectPickUpTest.ObjectPickUp += ObjectPickedUp;
        
        // if (combatTutorialTestEnable)
        // {
        //     startCombatUI.SetActive(true);
        //     CombatTestManager.Instance.combatUI = combatUI;
        //     CombatTestManager.Instance.finalCombatUI = tutorialEndUI;
        // }
        //else if (player != null) player.GetComponent<Combat>().enabled = false;
        
        onLevelLoadedEvent?.Invoke();
        enableEnemyPatrolEvent?.Invoke();
        if (currentScene == "FirstBossLevel")
        {
            stuckButton.interactable = false;
        }
        else stuckButton.interactable = true;

        switch (currentScene)
        {
            case "FirstBossLevel":
                StopCurrentMusic();
                StartCoroutine(FadeInTrack(bossMusic));
                bossMusic.start();
                player.GetComponent<Health>().currentHealth = playerHealthEndOfLevel;
                break;
            case "LevelGenTest":
                StopCurrentMusic();
                StartCoroutine(FadeInTrack(levelMusic));
                levelMusic.start();
                break;
            case "LevelSelectScene":
                StopCurrentMusic();
                StartCoroutine(FadeInTrack(mainMenuMusic));
                mainMenuMusic.start();
                break;
            case "DeathCutscene":
                StopCurrentMusic();
                StartCoroutine(FadeInTrack(levelMusic));
                levelMusic.start();
                break;
        }
    }

    private void StopCurrentMusic()
    {
        if (FmodExtensions.IsPlaying(mainMenuMusic))
        {
            StartCoroutine(FadeOutTrack(mainMenuMusic));
            //mainMenuMusic.stop(STOP_MODE.ALLOWFADEOUT);
        }
        if (FmodExtensions.IsPlaying(levelMusic))
        {
            StartCoroutine(FadeOutTrack(levelMusic));
            //levelMusic.stop(STOP_MODE.ALLOWFADEOUT);
        }
        if (FmodExtensions.IsPlaying(bossMusic))
        {
            StartCoroutine(FadeOutTrack(bossMusic));
            //bossMusic.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    private IEnumerator FadeInTrack(FMOD.Studio.EventInstance track)
    {
        // float targetVol = 0.0f;
        // track.getVolume(out targetVol);
        track.setVolume(0);
        float currentVol = 0.0f;
        track.getVolume(out currentVol);
        
        while (currentVol < 1)
        {
            track.setVolume(currentVol);
            currentVol += 0.1f;
            yield return new WaitForSeconds(0.2f);
        }

        currentVol = 0;
    }

    private IEnumerator FadeOutTrack(FMOD.Studio.EventInstance track)
    {
        float currentVol = 0.0f;
        track.getVolume(out currentVol);

        while (currentVol > 0)
        {
            track.setVolume(currentVol);
            currentVol -= 0.1f;
            yield return new WaitForSeconds(0.2f);
        }

        currentVol = 0;
    }

    private IEnumerator UnlockPlayer()
    {
        yield return new WaitForSeconds(1.5f);
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void EndLevel()
    {
        if (currentScene == "LevelGenTest")
        {
            playerHealthEndOfLevel = player.GetComponent<Health>().currentHealth;
            if(vcam1!=null)vcam1.transform.position = new Vector3(0, 0, -20);
            SceneManager.UnloadSceneAsync(currentScene);
            sceneToLoad = "FirstBossLevel";
            StartCoroutine(LoadLevelCoroutine());
        }

        if (currentScene == "FirstBossLevel")
        {
            statTracker.UpdateTimeElapsed(0,Time.time);
            SceneManager.UnloadSceneAsync(currentScene);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("LevelSelectScene"));
            currentScene = "LevelSelectScene";
            if(vcam1!=null)vcam1.transform.position = new Vector3(0, 0, -20);
            statsBG.SetActive(true);
            statsUI.SetActive(true);
            statTracker.DisplayStats();
        }

        if (currentScene is "OpeningCutscene" or "DeathCutscene")
        {
            SceneManager.UnloadSceneAsync(currentScene);
            sceneToLoad = "LevelGenTest";
            StartCoroutine(LoadLevelCoroutine());
        }
    }

    public IEnumerator DeathCutscene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.UnloadSceneAsync(currentScene);
        sceneToLoad = "DeathCutscene";
        StartCoroutine(LoadLevelCoroutine());
    }



    public void ToggleMap()
    {
        if (mapCameraVC != null && vcam1VC != null && currentScene == "LevelGenTest")
        {
            if (!isPaused)
            {
                if (!mapIsOpen)
                {
                    //open the map and trigger pause event to LOCK movement/timeScale
                    mapCameraVC.gameObject.SetActive(true);
                    mapCameraVC.gameObject.transform.position =
                        playerMapClone.transform.position + new Vector3(0, 0, -20);
                    vcam1VC.gameObject.SetActive(false);
                    pauseStartEvent?.Invoke();
                    mapOpenedEvent?.Invoke();
                }
                else
                {
                    //close the map and trigger pause event to UNLOCK movement/timeScale
                    vcam1VC.gameObject.SetActive(true);
                    mapCameraVC.gameObject.SetActive(false);
                    pauseEndEvent?.Invoke();
                    mapClosedEvent?.Invoke();
                }
                mapIsOpen = !mapIsOpen;
            }
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

        // if (combatTutorialTestEnable)
        // {
        //     startCombatUI.SetActive(false);
        //     enableEnemyPatrolEvent?.Invoke();
        // }
    }

    private void ObjectPickedUp()
    {
        // testObjectPickedUp = true;
        // if(zhiaSkeleton!=null) zhiaSkeleton.playerHasObject = true;
    }

    private void SomethingDied(GameObject deadThing)
    {
        if (deadThing.layer == 6)
        {
            StartCoroutine(DeathCutscene());
            statTracker.ResetOnDeathValues();
            //RespawnPlayer();
            // if(objectPickUpTest!=null)RespawnObject();
        }
        else if (deadThing.layer == 8)
        {
            statTracker.enemiesKilled++;
            deadThing.GetComponent<DamageAOETest>().enabled = false;
            deadThing.GetComponent<BasicEnemyPatrol>().enabled = false;
        }
    }

    public void DestroyFirstFloor()
    {
        // if (firstFloor != null) Destroy(firstFloor);
    }

    public void DestroySecondFloor()
    {
        // if (secondFloor != null) Destroy(secondFloor);
    }

    private void RespawnPlayer()
    {
        playerRespawnEvent?.Invoke();
    }

    private void RespawnObject()
    {
        // objectPickUpTest.gameObject.SetActive(true);
        // zhiaSkeleton.playerHasObject = false;
    }

    public void DialogueManagerEvent()
    {
        //tutorialStartUI.SetActive(true);
    }

    public void BeginTutorial()
    {
        //tutorialStartUI.SetActive(false);
        //tutorialDialogueFinishedEvent?.Invoke();
    }

    public void EndTutorial()
    {
        //tutorialEndUI.SetActive(true);
        //endTutorialEvent?.Invoke();
    }

    public void PauseUI()
    {
        if (!mapIsOpen)
        {
            if (isPaused)
            {
                pauseUI.SetActive(false);
                globalVolumeWithDoF.gameObject.SetActive(false);
                globalVolume.gameObject.SetActive(true);
                pauseEndEvent?.Invoke();
            }
            else if (!isPaused)
            {
                pauseUI.SetActive(true);
                globalVolume.gameObject.SetActive(false);
                globalVolumeWithDoF.gameObject.SetActive(true);
                pauseStartEvent?.Invoke();
            }
            isPaused = !isPaused;
        }
    }

    public void ExitLevel()
    {
        if (currentScene != "LevelSelectScene")
        {
            if(isPaused) PauseUI();
            //player.SetActive(false);
            player.GetComponent<Movement>().playerWalk.stop(STOP_MODE.ALLOWFADEOUT);
            //tutorialEndUI.SetActive(false);
            SceneManager.UnloadSceneAsync(currentScene);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("LevelSelectScene"));
            currentScene = "LevelSelectScene";
            statsBG.SetActive(true);
            statsUI.SetActive(true);
            statTracker.DisplayStats();
            vcam1.transform.position = new Vector3(0, 0, -20);
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
