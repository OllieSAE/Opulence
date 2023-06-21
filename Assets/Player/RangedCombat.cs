using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RangedCombat : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    
    public Animator animator;
    private bool currentlyAttacking;
    public float attackCooldown;
    public int rangedAttackPower;
    public Transform rangedAttackPoint;
    public float rangedAttackRange;
    public LayerMask enemyLayers;
    public float hitDelay;

    private void Awake()
    {
        if (gameObject.CompareTag("Player"))
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.Enable();
            playerInputActions.Player.RangedAttack.performed += RangedAttack;
        }
    }
    private void OnDisable()
    {
        playerInputActions.Player.RangedAttack.performed -= RangedAttack;
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        currentlyAttacking = false;
    }

    public void RangedAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartCoroutine(RangedAttackCoroutine());
        }
    }

    
    
    private IEnumerator RangedAttackCoroutine()
    {
        if (!currentlyAttacking)
        {
            currentlyAttacking = true;
            StartCoroutine(RangedAttackCooldownCoroutine());
            animator.SetTrigger("RangedAttack");

            yield return new WaitForSeconds(hitDelay);
            //Detect enemies in range of attack
            
            //Damage them
            
        }
    }
    
    private IEnumerator RangedAttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        
        currentlyAttacking = false;
    }
}
