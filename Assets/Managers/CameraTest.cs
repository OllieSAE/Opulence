using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTest : MonoBehaviour
{
    public Transform target;
    private Transform myTransform;
    private float posX;
    private float posY;
    private float posZ;

    private void Awake()
    {
        myTransform = transform;
    }

    private void FixedUpdate()
    {
        posX = myTransform.position.x;
        posY = myTransform.position.y;
        posZ = myTransform.position.z;
        myTransform.position = Vector3.Lerp(new Vector3(posX,posY,posZ),new Vector3(target.position.x, target.position.y,posZ),0.25f);
    }
}
