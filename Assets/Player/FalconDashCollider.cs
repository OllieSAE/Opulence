using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalconDashCollider : MonoBehaviour
{
    private int meleeAttackPower;
    private Combat combat;

    private void Start()
    {
        combat = GetComponentInParent<Combat>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            meleeAttackPower = combat.meleeAttackPower;
            other.GetComponentInParent<Health>().ChangeHealth(-meleeAttackPower,this.gameObject);
        }
    }
}
