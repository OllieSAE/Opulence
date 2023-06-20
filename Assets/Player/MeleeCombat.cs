using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeCombat : MonoBehaviour
{
    public Animator animator;
    private bool currentlyAttacking;
    public float attackCooldown;
    public int meleeAttackPower;
    public Transform meleeAttackPoint;
    public float meleeAttackRange;
    public LayerMask enemyLayers;
    
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        currentlyAttacking = false;
    }
    
    void Update()
    {
        //replace with new input system shenanigans
        if (Input.GetMouseButtonDown(0))
        {
            MeleeAttack();
        }
    }

    void MeleeAttack()
    {
        if (!currentlyAttacking)
        {
            currentlyAttacking = true;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            
            animator.SetTrigger("MeleeAttack");
            
            //Detect enemies in range of attack
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(meleeAttackPoint.position, meleeAttackRange, enemyLayers);

            //Damage them
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponentInParent<Health>().ChangeHealth(-meleeAttackPower,this.gameObject);
            }
        }
    }

    private IEnumerator MeleeAttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        currentlyAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRange);
    }
}
