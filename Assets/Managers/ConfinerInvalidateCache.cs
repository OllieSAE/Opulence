using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ConfinerInvalidateCache : MonoBehaviour
{
    public CinemachineConfiner2D confiner;
    void Start()
    {
        StartCoroutine(InvalidateCache());
    }

    public IEnumerator InvalidateCache()
    {
        yield return new WaitForSeconds(1f);
        confiner.InvalidateCache();
    }
}
