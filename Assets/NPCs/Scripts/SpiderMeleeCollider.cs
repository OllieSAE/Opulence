using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMeleeCollider : MonoBehaviour
{
    private int meleeAttackPower;
    public Combat combat;
    public Vector2 overlapBoxSize;
    public Vector2 overlapBoxPos;
    public Vector2 newPos;
    private bool damageDealt = false;
    
    public int damageModifier = 1;

    private void OnEnable()
    {
        meleeAttackPower = combat.meleeAttackPower;
        damageDealt = false;
    }

    private void Update()
    {
        newPos = overlapBoxPos;
        newPos.x += transform.position.x;
        newPos.y += transform.position.y;
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(newPos, overlapBoxSize, 0.5f);
        foreach (Collider2D hitPlayer in hitPlayers)
            if (hitPlayer.CompareTag("Player") && !damageDealt)
            {
                damageDealt = true;
                hitPlayer.GetComponent<Health>().ChangeHealth(-meleeAttackPower*damageModifier,this.gameObject);
            }
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawCube(newPos,overlapBoxSize);
    // }
}
