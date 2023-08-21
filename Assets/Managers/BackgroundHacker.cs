using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundHacker : MonoBehaviour
{
    public GameObject mainCamera;
    public float zPos;

    private void OnEnable()
    {
        transform.position = new Vector3(
            mainCamera.transform.position.x,
            mainCamera.transform.position.y,
            zPos);
    }

    private void Update()
    {
        transform.position = new Vector3(
            mainCamera.transform.position.x,
            mainCamera.transform.position.y,
            zPos);
    }
}
