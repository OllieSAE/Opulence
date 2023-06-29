using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public Scene movementLevel;
    public Scene combatLevel;
    public string sceneToLoad;
    public GameObject mainMenuCanvas;

    private static MainMenuManager _instance;
    public static MainMenuManager Instance
    {
        get
        {
            if (_instance == null)
            {
                print("Main Menu Manager is null!");
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        //enable menu canvas
    }

    public void SelectMovementLevel()
    {
        sceneToLoad = "MovementTestScene";
        StartCoroutine(LoadLevelCoroutine());
    }

    public void SelectCombatLevel()
    {
        sceneToLoad = "CombatTestScene";
        StartCoroutine(LoadLevelCoroutine());
    }

    private IEnumerator LoadLevelCoroutine()
    {
        mainMenuCanvas.SetActive(false);
        print("loading level CR started");
        yield return new WaitForSeconds(1f);
        LoadSceneAsync();
    }
    
    public void LoadSceneAsync()
    {
        SceneManager.LoadSceneAsync(sceneToLoad,LoadSceneMode.Additive);
    }
}
