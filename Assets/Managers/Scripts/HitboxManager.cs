using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : MonoBehaviour
{
    [Header("General")]
    public LayerMask enemyLayer;
    public LayerMask playerLayer;
    public Combat parentCombat;
    [Header("Falcon Attack 1 Frames")]
    public Transform falconA1F1;
    public Transform falconA1F2;
    public Transform falconA1F3;
    public Transform falconA1F4;
    public float falconA1Radius;
    [Header("Falcon Attack 2 Frames")]
    public Transform falconA2F1;
    public Transform falconA2F2;
    public Transform falconA2F3;
    public Transform falconA2F4;
    public float falconA2Radius;
    [Header("Falcon Attack 3 Frames")]
    public Transform falconA3F1;
    public Transform falconA3F2;
    public Transform falconA3F3;
    public Transform falconA3F4;
    public float falconA3Radius;
    [Header("Spider Attack Frames")]
    public Transform spiderFrame1;

    //"Global" function for dealing damage via hitbox
    public void GenerateHitboxForDamage(Vector3 position, float size, int damageAmount)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(position, size, enemyLayer);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponentInParent<Health>().ChangeHealth(-damageAmount,this.gameObject);
        }
    }

    #region Player Attacks

    #region Falcon Attacks

    #region Falcon Attack 1 Frames

    public void FalconAttack1Frame1()
    {
        GenerateHitboxForDamage(falconA1F1.position,falconA1Radius,parentCombat.meleeAttackPower);
    }

    public void FalconAttack1Frame2()
    {
        GenerateHitboxForDamage(falconA1F2.position,falconA1Radius,parentCombat.meleeAttackPower);
    }

    public void FalconAttack1Frame3()
    {
        GenerateHitboxForDamage(falconA1F3.position,falconA1Radius,parentCombat.meleeAttackPower);
    }

    public void FalconAttack1Frame4()
    {
        GenerateHitboxForDamage(falconA1F4.position,falconA1Radius,parentCombat.meleeAttackPower);
    }

    #endregion

    #region Falcon Attack 2 Frames

    public void FalconAttack2Frame1()
    {
        GenerateHitboxForDamage(falconA2F1.position,falconA2Radius,parentCombat.meleeAttackPower);
    }

    public void FalconAttack2Frame2()
    {
        GenerateHitboxForDamage(falconA2F2.position,falconA2Radius,parentCombat.meleeAttackPower);
    }

    public void FalconAttack2Frame3()
    {
        GenerateHitboxForDamage(falconA2F3.position,falconA2Radius,parentCombat.meleeAttackPower);
    }

    public void FalconAttack2Frame4()
    {
        GenerateHitboxForDamage(falconA2F4.position,falconA2Radius,parentCombat.meleeAttackPower);
    }

    #endregion

    #region Falcon Attack 3 Frames

    public void FalconAttack3Frame1()
    {
        GenerateHitboxForDamage(falconA3F1.position,falconA3Radius,parentCombat.meleeAttackPower);
    }

    public void FalconAttack3Frame2()
    {
        GenerateHitboxForDamage(falconA3F2.position,falconA3Radius,parentCombat.meleeAttackPower);
    }

    public void FalconAttack3Frame3()
    {
        GenerateHitboxForDamage(falconA3F3.position,falconA3Radius,parentCombat.meleeAttackPower);
    }

    public void FalconAttack3Frame4()
    {
        GenerateHitboxForDamage(falconA3F4.position,falconA3Radius,parentCombat.meleeAttackPower);
    }

    #endregion
    
    #endregion

    #endregion

    

    #region Enemy Attacks

    #region Basic Melee Attack

    public void BasicMeleeAttack()
    {
        
    }

    #endregion

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(falconA1F1.position, falconA1Radius);
        Gizmos.DrawWireSphere(falconA1F2.position, falconA1Radius);
        Gizmos.DrawWireSphere(falconA1F3.position, falconA1Radius);
        Gizmos.DrawWireSphere(falconA1F4.position, falconA1Radius);
    }
}
