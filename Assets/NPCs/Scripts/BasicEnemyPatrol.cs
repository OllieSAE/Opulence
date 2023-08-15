using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class BasicEnemyPatrol : MonoBehaviour
{
    [Header ("Environment Checks")]
    public Transform groundAheadCheck;
    public Transform wallAheadCheck;
    public float groundCheckRadius;
    public float wallCheckRadius;
    private LayerMask groundLayer;
    private LayerMask playerLayer;
    private LayerMask playerAndGroundLayer;
    private bool isGroundAhead;
    private bool isWallAhead;
    private Rigidbody2D rigidbody;
    private Animator animator;
    private Combat combat;
    private bool facingLeft;
    private bool flipCR;
    private bool patrolling = true;
    private bool bossRayStarted = false;

    [Header("Move/Combat Stuff")]
    public float defaultSpeed;
    public float aggroSpeed;
    public float currentSpeed;
    public bool patrolOnly;
    public float attackDelay;
    public float sightDistance;
    public float rearSightDistance;
    public float rearViewFlipDelay;
    public float attackRange;
    public float meleeAttackRange;
    public bool isPlayerInSight;
    public bool isPlayerInRange;
    public bool isPlayerInMeleeRange;
    public bool isTransitioning;
    private bool isAttacking = false;
    private bool attackCD = false;
    public float attackCooldown;

    [Header("Boss Stuff")]
    public GameObject player;
    public int playerHiding = 0;
    public bool bossCharging = false;
    public float bossChargeSpeed;
    public Vector3 targetDirRaw;
    public Vector3 targetDir;

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
        playerAndGroundLayer = LayerMask.GetMask("Ground", "Player");
        isPlayerInRange = false;
        isPlayerInMeleeRange = false;
        flipCR = false;
        currentSpeed = defaultSpeed;
        isTransitioning = false;
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
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(groundAheadCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(wallAheadCheck.position, wallCheckRadius);
    }

    private void FixedUpdate()
    {
        if (!patrolOnly && enemyType != EnemyType.Boss)
        {
            RaycastHit2D frontHit = Physics2D.Raycast(
                origin: transform.position,
                direction: new Vector2(transform.localScale.x, 0),
                distance: sightDistance,
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

            #region Rear View Vision + Flip
            // RaycastHit2D backHit = Physics2D.Raycast(
            //     origin: transform.position,
            //     direction: new Vector2(-transform.localScale.x, 0),
            //     distance: rearSightDistance,
            //     layerMask: playerLayer);
            // if (backHit.collider != null && enemyType != EnemyType.Charger)
            // {
            //     StartCoroutine(FlipCoroutine());
            //     //Flip();
            // }
            #endregion
        
            //Just to visualize the direction/length of the Ray
            Vector3 forward = new Vector3(transform.localScale.x, 0, 0);
            Debug.DrawRay(wallAheadCheck.position,forward * sightDistance);
            //Debug.DrawRay(wallAheadCheck.position,-forward * rearSightDistance);
        }

        if (enemyType == EnemyType.Boss)
        {
            StartCoroutine(BossRayCoroutine());
        }
    }

    private IEnumerator BossRayCoroutine()
    {
        
        if (!bossRayStarted)
        {
            bossRayStarted = true;
            targetDirRaw = (player.transform.position - transform.position);
            targetDir = targetDirRaw.normalized;
            targetDir.y = Mathf.Clamp(targetDir.y, -0.55f, 0.00f);
            RaycastHit2D frontHit = Physics2D.Raycast(
                origin: transform.position,
                direction: new Vector2(targetDir.x, targetDir.y),
                distance: sightDistance,
                layerMask: playerAndGroundLayer);
            
            if (frontHit.collider != null && frontHit.collider.CompareTag("Player"))
            {
                isPlayerInSight = true;
                playerHiding = 0;
                float distance = Vector2.Distance(transform.position, frontHit.collider.gameObject.transform.position);
                
                if(distance < meleeAttackRange && distance > -meleeAttackRange && !flipCR)
                {
                    isPlayerInMeleeRange = true;
                }
                else isPlayerInMeleeRange = false;
                
                if ((distance < attackRange && distance > -attackRange) && !flipCR)
                {
                    isPlayerInRange = true;
                }
                else isPlayerInRange = false;
            }
            else if (frontHit.collider == null)
            {
                isPlayerInSight = false;
                isPlayerInRange = false;
                isPlayerInMeleeRange = false;
                playerHiding++;
                if (playerHiding > 5)
                {
                    print("YOU CANNOT HIDE FROM ME");
                    combat.SpiderAttackOverride();
                    playerHiding = 0;
                    isAttacking = true;
                }
            }
            Debug.DrawLine(transform.position,transform.position+targetDir*sightDistance,Color.red);
            yield return new WaitForSeconds(1f);
            bossRayStarted = false;
        }
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
        if (isPlayerInRange && !isAttacking && !patrolOnly)
        {
            if (enemyType != EnemyType.Boss)
            {
                StartCoroutine(EnemyAttackCoroutine());
                currentSpeed = aggroSpeed;
            }
            else
            {
                BossCheckDirection();
                StartCoroutine(BossAttackCoroutine());
                //need to make the boss move after casting
            }
        }
        
        else if (isTransitioning)
        {
            //if (!isPlayerInRange) isAttacking = false;
        }
        else if (isAttacking && isGroundAhead && !isWallAhead && enemyType == EnemyType.Charger)
        {
            transform.Translate(targetDirection * currentSpeed * Time.deltaTime);
        }
        else if (bossCharging && isGroundAhead && !isWallAhead && enemyType == EnemyType.Boss)
        {
            transform.Translate(targetDirection * bossChargeSpeed * Time.deltaTime);
        }
        else if (isGroundAhead && !isWallAhead && !isPlayerInRange && !isAttacking)
        {
            if(enemyType==EnemyType.Boss) BossCheckDirection();
            transform.Translate(targetDirection * currentSpeed * Time.deltaTime);
            if (enemyType == EnemyType.Boss)
            {
                animator.SetBool("Walking", true);
            }
            else animator.SetBool("Running", true);
        }
        else if (isGroundAhead && !isWallAhead && attackCD && !isPlayerInMeleeRange)
        {
            if(enemyType==EnemyType.Boss) BossCheckDirection();
            transform.Translate(targetDirection * aggroSpeed * Time.deltaTime);
            animator.SetBool("Walking", true);
        }
        else if (isAttacking && enemyType == EnemyType.Boss)
        {
            animator.SetBool("Walking", false);
        }
        else if ((!isGroundAhead || isWallAhead) && !isPlayerInRange)
        {
            Flip();
        }
    }

    private void BossCheckDirection()
    {
        if (isPlayerInSight)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            if(dir.x * transform.localScale.x < 0 && dir.y < 0.9) Flip();
        }
    }

    public void BossEndAttackAnimation()
    {
        attackCD = true;
        StartCoroutine(BossEndAttackAnimationCoroutine());
    }

    private IEnumerator BossEndAttackAnimationCoroutine()
    {
        yield return new WaitForSeconds(1f);
        isAttacking = false;
        attackCD = false;
    }

    private IEnumerator BossAttackCoroutine()
    {
        animator.SetBool("Walking", false);
        isAttacking = true;
        if (isPlayerInMeleeRange)
        {
            combat.EnemyAttack(enemyType, aggroSpeed, isPlayerInMeleeRange);
            yield return new WaitForSeconds(attackDelay);
        }
        else
        {
            combat.EnemyAttack(enemyType, aggroSpeed, isPlayerInMeleeRange);
            yield return new WaitForSeconds(attackDelay);
        }
        currentSpeed = defaultSpeed;
    }

    private IEnumerator EnemyAttackCoroutine()
    {
        animator.SetBool("Running", false);
        combat.EnemyAttack(enemyType, aggroSpeed, isPlayerInMeleeRange);
        if (enemyType == EnemyType.Charger)
        {
            isTransitioning = true;
            yield return new WaitForSeconds(combat.chargeTransitionDelay);
            isTransitioning = false;
        }
        isAttacking = true;
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
