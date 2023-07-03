using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class OnLevelLoadFixer : MonoBehaviour
{
    private CinemachineBrain cinemachineBrain;
    private CinemachineVirtualCamera camera;
    private CinemachineConfiner2D confiner;

    private void Start()
    {
        GameManager.Instance.onLevelLoadedEvent += OnLevelLoad;
        try
        {
            GetComponent<CinemachineVirtualCamera>().m_Follow =
                GameObject.FindGameObjectWithTag("Player").gameObject.transform;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.onLevelLoadedEvent -= OnLevelLoad;
    }

    private void OnLevelLoad()
    {
        cinemachineBrain = GetComponent<CinemachineBrain>();
        camera = GetComponent<CinemachineVirtualCamera>();
        confiner = GetComponent<CinemachineConfiner2D>();

        if (camera != null) camera.m_Follow = GameManager.Instance.player.transform;

        if (confiner != null) StartCoroutine(InvalidateCache());
    }
    
    public IEnumerator InvalidateCache()
    {
        yield return new WaitForSeconds(1f);
        confiner.InvalidateCache();
    }
}
