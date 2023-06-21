using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeCombat : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    
    public Animator animator;
    private bool currentlyAttacking;
    public float attackCooldown;
    public int meleeAttackPower;
    public Transform meleeAttackPoint;
    public float meleeAttackRange;
    public LayerMask enemyLayers;
    public float hitDelay;

    private void Awake()
    {
        if (gameObject.CompareTag("Player"))
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.Enable();
            playerInputActions.Player.MeleeAttack.performed += MeleeAttack;
        }
    }

    private void OnDisable()
    {
        playerInputActions.Player.MeleeAttack.performed -= MeleeAttack;
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        currentlyAttacking = false;
    }
    
    void Update()
    {
        //enemy AI stuff
    }

    public void MeleeAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartCoroutine(MeleeAttackCoroutine());
        }
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        if (!currentlyAttacking)
        {
            currentlyAttacking = true;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            animator.SetTrigger("MeleeAttack");

            yield return new WaitForSeconds(hitDelay);
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
