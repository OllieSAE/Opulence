using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public ObjectPickUpTest objectPickUpTest;
    public bool testObjectPickedUp;
    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        testObjectPickedUp = false;
    }

    //this will break when GM exists before level loaded
    private void OnEnable()
    {
        objectPickUpTest.ObjectPickUp += ObjectPickedUp;
    }

    private void ObjectPickedUp()
    {
        testObjectPickedUp = true;
    }
}
