using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMeleeCollider : MonoBehaviour
{
    private int meleeAttackPower;
    private Combat combat;
    public BoxCollider2D collider;
    
    public int damageModifier = 1;

    private void Start()
    {
        combat = GetComponentInParent<Combat>();
    }

    private void OnEnable()
    {
        collider.enabled = true;
    }

    //this DOES NOT WORK if the player is stationary
    //because unity is doing a bit... again
    //maybe attempt a physics2D circle check, like the original charger?
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            meleeAttackPower = combat.meleeAttackPower;
            other.GetComponentInParent<Health>().ChangeHealth(-meleeAttackPower * damageModifier,this.gameObject);
            gameObject.SetActive(false);
        }
    }
}
