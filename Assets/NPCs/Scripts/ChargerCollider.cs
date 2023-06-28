using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargerCollider : MonoBehaviour
{
    private int meleeAttackPower;
    private Combat combat;

    private void Start()
    {
        combat = GetComponentInParent<Combat>();
    }

    private void Update()
    {
        meleeAttackPower = combat.meleeAttackPower;
        Collider2D hitEnemy = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Player"));
        if (hitEnemy != null)
        {
            print("i charged into something and dealt dmg x " + meleeAttackPower);
            hitEnemy.GetComponentInParent<Health>().ChangeHealth(-meleeAttackPower,this.gameObject);
            gameObject.SetActive(false);
        }
    }
}
