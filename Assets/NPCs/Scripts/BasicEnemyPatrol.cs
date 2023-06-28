using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicEnemyPatrol : MonoBehaviour
{
    [Header ("Environment Checks")]
    public Transform groundAheadCheck;
    public Transform wallAheadCheck;
    public float groundCheckRadius;
    public float wallCheckRadius;
    private LayerMask groundLayer;
    private LayerMask playerLayer;
    private bool isGroundAhead;
    private bool isWallAhead;
    private Rigidbody2D rigidbody;
    private Animator animator;
    private Combat combat;
    private bool facingLeft;
    private bool flipCR;
    private bool patrolling = false;
    private bool isAttacking = false;

    [Header("Move/Combat Stuff")]
    public float defaultSpeed;
    public float aggroSpeed;
    public float currentSpeed;
    public float attackDelay;
    public float sightDistance;
    public float rearSightDistance;
    public float rearViewFlipDelay;
    public float attackRange;
    public bool isPlayerInSight;
    public bool isPlayerInRange;
    
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
        isPlayerInRange = false;
        flipCR = false;
        currentSpeed = defaultSpeed;
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
        //isPlayerAhead = Physics2D.OverlapCircle(wallAheadCheck.position,wallCheckRadius, playerLayer);
        if(patrolling) Patrol();
    }

    private void FixedUpdate()
    {
        RaycastHit2D frontHit = Physics2D.Raycast(
            origin: transform.position,
            direction: new Vector2(transform.localScale.x, 0),
            distance: sightDistance,
            layerMask: playerLayer);
        RaycastHit2D backHit = Physics2D.Raycast(
            origin: transform.position,
            direction: new Vector2(-transform.localScale.x, 0),
            distance: rearSightDistance,
            layerMask: playerLayer);
        
        if (frontHit.collider != null)
        {
            isPlayerInSight = true;
            
            if(enemyType != EnemyType.Charger) currentSpeed = aggroSpeed;
            
            float distance = Vector2.Distance(transform.position, frontHit.collider.gameObject.transform.position);
            if ((distance < attackRange && distance > -attackRange) && !flipCR)
            {
                isPlayerInRange = true;
            }
            else isPlayerInRange = false;
        }
        else
        {
            isPlayerInSight = false;
            isPlayerInRange = false;
            if(enemyType != EnemyType.Charger) currentSpeed = defaultSpeed;
        }

        if (backHit.collider != null && enemyType != EnemyType.Charger)
        {
            StartCoroutine(FlipCoroutine());
            //Flip();
        }
        
        //Just to visualize the direction/length of the Ray
        Vector3 forward = new Vector3(transform.localScale.x, 0, 0);
        Debug.DrawRay(wallAheadCheck.position,forward * sightDistance);
        Debug.DrawRay(wallAheadCheck.position,-forward * rearSightDistance);
    }

    private IEnumerator FlipCoroutine()
    {
        if (!flipCR)
        {
            print("flip CR");
            flipCR = true;
            Flip();
            yield return new WaitForSeconds(rearViewFlipDelay);
            flipCR = false;
        }
    }

    void Patrol()
    {
        Vector3 targetDirection = new Vector3(transform.localScale.x, 0, 0);
        if (!isGroundAhead || isWallAhead)
        {
            currentSpeed = defaultSpeed;
        }
        if (isPlayerInRange && !isAttacking)
        {
            StartCoroutine(EnemyAttackCoroutine());
            currentSpeed = aggroSpeed;
        }
        else if (isAttacking && isGroundAhead && !isWallAhead && enemyType == EnemyType.Charger)
        {
            transform.Translate(targetDirection * currentSpeed * Time.deltaTime);
        }
        else if (isGroundAhead && !isWallAhead && !isPlayerInRange && !isAttacking)
        {
            transform.Translate(targetDirection * currentSpeed * Time.deltaTime);
            animator.SetBool("Running", true);
        }
        else if ((!isGroundAhead || isWallAhead) && !isPlayerInRange)
        {
            Flip();
        }
    }

    private IEnumerator EnemyAttackCoroutine()
    {
        isAttacking = true;
        animator.SetBool("Running", false);
        combat.EnemyAttack(enemyType, aggroSpeed);
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
        currentSpeed = defaultSpeed;
    }

    void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
