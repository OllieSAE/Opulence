using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingTextAnimation : MonoBehaviour
{
    public GameObject[] loadingTexts;

    public float duration;

    public bool currentlyRunning;

    private IEnumerator amLoadingTheText;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < loadingTexts.Length; i++)
        {
            loadingTexts[i].SetActive(false);
        }

        currentlyRunning = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!currentlyRunning)
        {
            StartCoroutine(LoadingTheText());
        }
        
    }
    private IEnumerator LoadingTheText()
    {

        currentlyRunning = true;
        for (int i = 0; i < loadingTexts.Length; i++)
        {
            loadingTexts[i].SetActive(true);
            yield return new WaitForSeconds(duration);
            loadingTexts[i].SetActive(false);
        }
        currentlyRunning = false;

    }
}
