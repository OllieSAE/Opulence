using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicEnemyPatrol : MonoBehaviour
{
    public Transform groundAheadCheck;
    public Transform wallAheadCheck;
    public float groundCheckRadius;
    public float wallCheckRadius;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public bool isGroundAhead;
    public bool isWallAhead;
    public bool isPlayerAhead;
    private Rigidbody2D rigidbody;
    private Animator animator;
    private Combat combat;
    public bool facingLeft;
    public bool patrolling = false;
    public bool isAttacking = false;
    public float attackDelay;
    
    public enum EnemyType
    {
        Melee,
        Ranged,
        Charger,
        Boss
    }

    [SerializeField] public EnemyType enemyType;
    
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        combat = GetComponent<Combat>();
        GameManager.Instance.enableEnemyPatrolEvent += EnablePatrolling;
        GameManager.Instance.disableEnemyPatrolEvent += DisablePatrolling;
        groundLayer = LayerMask.GetMask("Ground");
        playerLayer = LayerMask.GetMask("Player");
        isPlayerAhead = false;
    }

    private void OnDisable()
    {
        GameManager.Instance.enableEnemyPatrolEvent -= EnablePatrolling;
        GameManager.Instance.disableEnemyPatrolEvent -= DisablePatrolling;
    }

    void EnablePatrolling()
    {
        patrolling = true;
    }

    void DisablePatrolling()
    {
        patrolling = false;
        animator.SetBool("Running", false);
    }

    void Update()
    {
        isGroundAhead = Physics2D.OverlapCircle(groundAheadCheck.position,groundCheckRadius, groundLayer);
        isWallAhead = Physics2D.OverlapCircle(wallAheadCheck.position,wallCheckRadius, groundLayer);
        isPlayerAhead = Physics2D.OverlapCircle(wallAheadCheck.position,wallCheckRadius, playerLayer);
        if(patrolling) Patrol();
    }

    void Patrol()
    {
        if (isPlayerAhead && !isAttacking)
        {
            StartCoroutine(EnemyAttackCoroutine());
        }
        else if (isGroundAhead && !facingLeft && !isWallAhead && !isPlayerAhead)
        {
            transform.Translate(Vector3.right * Time.deltaTime);
            animator.SetBool("Running", true);
            //animator.SetBool("Attack", false);
        }
        else if (isGroundAhead && facingLeft && !isWallAhead && !isPlayerAhead)
        {
            transform.Translate(Vector3.left * Time.deltaTime);
            animator.SetBool("Running", true);
            //animator.SetBool("Attack", false);
        }
        else if ((!isGroundAhead || isWallAhead) && !isPlayerAhead)
        {
            Flip();
            //animator.SetBool("Attack", false);
        }
    }

    private IEnumerator EnemyAttackCoroutine()
    {
        isAttacking = true;
        combat.EnemyAttack(enemyType);
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
    }

    void Flip()
    {
        print("tried to flip");
        facingLeft = !facingLeft;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
