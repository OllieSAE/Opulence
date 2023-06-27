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
    private bool patrolling = false;
    private bool isAttacking = false;

    [Header("Move/Combat Stuff")]
    public float defaultSpeed;
    public float aggroSpeed;
    private float currentSpeed;
    public float attackDelay;
    public float sightDistance;
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
        RaycastHit2D hit = Physics2D.Raycast(
            origin: transform.position,
            direction: new Vector2(transform.localScale.x, 0),
            distance: sightDistance,
            layerMask: playerLayer);
        
        if (hit.collider != null)
        {
            isPlayerInSight = true;
            //need some sort of visual feedback indicating this Enemy can see the Player
            //maybe change speed?
            currentSpeed = aggroSpeed;
            
            float distance = Vector2.Distance(transform.position, hit.collider.gameObject.transform.position);
            if (distance < attackRange && distance > -attackRange)
            {
                isPlayerInRange = true;
            }
            
            //print("distance to " + hit.collider.name + " is " + distance);
        }
        else
        {
            isPlayerInSight = false;
            currentSpeed = defaultSpeed;
            isPlayerInRange = false;
        }
        
        //Just to visualize the direction/length of the Ray
        Vector3 forward = new Vector3(transform.localScale.x, 0, 0);
        Debug.DrawRay(wallAheadCheck.position,forward * sightDistance);
    }

    void Patrol()
    {
        if (isPlayerInRange && !isAttacking)
        {
            StartCoroutine(EnemyAttackCoroutine());
        }
        else if (isGroundAhead && !facingLeft && !isWallAhead && !isPlayerInRange)
        {
            transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
            animator.SetBool("Running", true);
        }
        else if (isGroundAhead && facingLeft && !isWallAhead && !isPlayerInRange)
        {
            transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);
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
