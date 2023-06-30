using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPickUpTest : MonoBehaviour
{
    public AnimationCurve myCurve;

    private float posX;
    private float posY;

    public event Action ObjectPickUp;
    
    private void Start()
    {
        posX = transform.position.x;
        posY = transform.position.y;
    }

    private void Update()
    {
        transform.position = new Vector2(posX, posY + myCurve.Evaluate((Time.time * myCurve.length)));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ObjectPickUp?.Invoke();
            RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Item Pickup");
            this.gameObject.SetActive(false);
        }
    }
}
