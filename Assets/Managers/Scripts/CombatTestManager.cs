using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTestManager : MonoBehaviour
{
    public List<GameObject> trainingDummies = new List<GameObject>();
    public List<GameObject> patrolEnemies = new List<GameObject>();
    public List<GameObject> hostileEnemies = new List<GameObject>();

    public GameObject combatUI;
    public GameObject finalCombatUI;
    
    private static CombatTestManager _instance;

    public static CombatTestManager Instance
    {
        get
        {
            if (_instance == null)
            {
                print("Combat Test Manager is null!");
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
        GameManager.Instance.onLevelLoadedEvent += OnLevelLoad;
    }

    private void OnDisable()
    {
        GameManager.Instance.onLevelLoadedEvent -= OnLevelLoad;
    }

    private void OnLevelLoad()
    {
        combatUI.SetActive(false);
        finalCombatUI.SetActive(false);
    }

    public void KilledEnemy(GameObject enemy)
    {
        if (trainingDummies.Contains(enemy))
        {
            trainingDummies.Remove(enemy);
            if(TrainingDummiesDead()) StartCoroutine(DestroyFirstFloor());
        }

        if (patrolEnemies.Contains(enemy))
        {
            patrolEnemies.Remove(enemy);
            if (PatrolEnemiesDead()) StartCoroutine(DestroySecondFloor());
        }

        if (hostileEnemies.Contains(enemy))
        {
            hostileEnemies.Remove(enemy);
            if (HostileEnemiesDead()) CombatTrainingComplete();
        }
    }

    public bool TrainingDummiesDead()
    {
        if (trainingDummies.Count <= 0)
        {
            StartCoroutine(DestroyFirstFloor());
            return true;
        }
        else return false;
    }
    
    public bool PatrolEnemiesDead()
    {
        if (patrolEnemies.Count <= 0)
        {
            StartCoroutine(DestroySecondFloor());
            return true;
        }
        else return false;
    }
    
    public bool HostileEnemiesDead()
    {
        if (hostileEnemies.Count <= 0)
        {
            return true;
        }
        else return false;
    }


    private IEnumerator DestroyFirstFloor()
    {
        combatUI.SetActive(true);
        yield return new WaitForSeconds(3.2f);
        combatUI.SetActive(false);
        GameManager.Instance.DestroyFirstFloor();
    }

    private IEnumerator DestroySecondFloor()
    {
        combatUI.SetActive(true);
        yield return new WaitForSeconds(3.2f);
        combatUI.SetActive(false);
        GameManager.Instance.DestroySecondFloor();
    }

    private void CombatTrainingComplete()
    {
        finalCombatUI.SetActive(true);
    }
}
