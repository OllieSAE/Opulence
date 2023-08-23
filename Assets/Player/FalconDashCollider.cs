using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalconDashCollider : MonoBehaviour
{
    private int falconDashAttackPower;
    private Combat combat;

    private void Start()
    {
        combat = GetComponentInParent<Combat>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            falconDashAttackPower = combat.rangedAttackPower;
            other.GetComponentInParent<Health>().ChangeHealth(-falconDashAttackPower,this.gameObject);
        }
    }
}
