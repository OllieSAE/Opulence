using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    
    private Vector2 moveSpeed = new Vector2(0, 0);
    private GameObject owner;
    private int damage;
    private float flightTime;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rigidbody.velocity = new Vector2(moveSpeed.x * transform.localScale.x, moveSpeed.y);
        StartCoroutine(DestroySelf());
    }

    public void SetProjectileValues(float newFlightTime, GameObject newOwner, Vector2 newMoveSpeed, int newDamage)
    {
        flightTime = newFlightTime;
        owner = newOwner;
        moveSpeed = newMoveSpeed;
        damage = newDamage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Health>() != null && other.gameObject.layer != owner.gameObject.layer)
        {
            Health enemyHealth = other.GetComponent<Health>();
            enemyHealth.ChangeHealth(-damage,owner);
            print("i hit something");
            Destroy(this.gameObject);
        }
    }

    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(flightTime);
        Destroy(this.gameObject);
    }
}
