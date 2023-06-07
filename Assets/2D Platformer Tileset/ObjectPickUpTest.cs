using System;
using System.Collections;
using System.Collections.Generic;
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
        if (other.GetComponentInParent<PlayerInput>() != null)
        {
            ObjectPickUp?.Invoke();
            Destroy(this.gameObject);
        }
    }
}
