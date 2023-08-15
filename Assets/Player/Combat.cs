using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = System.Random;

public class Combat : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    private Animator animator;
    private DamageAOETest damageAoeTest;
    private Movement movement;
    private BasicEnemyPatrol basicEnemyPatrol;

    [Header("General Combat")]
    public bool currentlyAttacking;
    public float attackCooldown;
    private LayerMask enemyLayer;
    private LayerMask playerLayer;
    private Health health;
    private float meleeComboTimer;
    private bool canFirstCombo;
    private bool canSecondCombo;
    //private BasicEnemyPatrol basicEnemyPatrol;

    [Header("Melee Combat")]
    public int meleeAttackPower;
    public Transform meleeAttackPoint;
    public float meleeAttackRange;
    public float meleeHitDelay;
    public float chargeTransitionDelay;
    public float meleeComboTimerCutoff;
    public List<AttackSO> meleeCombo;
    private float lastComboEnd;
    private float lastClickedTime;
    private int comboCounter;
    public float internalComboTimeDelay;
    public float externalComboTimeDelay;
    public float whatDoICallThis;

    //this is set as BasicEnemyPatrol "Aggro Speed" on the Charger Prefab
    private float chargerAttackSpeed;

    [Header("Ranged Combat")]
    public int rangedAttackPower;
    public int rangedMaxAmmo;
    private int rangedCurrentAmmo;
    public float rechargeAmmoTime;
    private AmmoBar ammoBar; 
    public Transform launchPoint;
    public float flightTime;
    public float rangedHitDelay;
    public GameObject defaultProjectilePrefab;
    public GameObject webBlastProjectile;
    public GameObject venomSpitProjectile;
    public Vector2 projectileSpeed;
    private bool rechargingAmmo;

    [Header("Charger Enemy Combat")]
    public GameObject chargerCollider;

    [Header("Boss Combat")]
    public float specialAttackHitDelay;
    public GameObject spiderBossTileCollider;
    public GameObject crashDownCollider;
    public float climbUpSpeed;
    public float crashDownSpeed;
    public int crashDownPower;

    [Header("Mask Type")] 
    public bool falconMask;
    public bool spiderMask;

    [Header("Falcon Combat")] 
    public bool falconDashCD;
    public GameObject falconDashCollider;

    private void Awake()
    {
        if (gameObject.CompareTag("Player"))
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.Enable();
            playerInputActions.Player.MeleeAttack.performed += MeleeAttack;
            playerInputActions.Player.RangedAttack.performed += RangedAttack;
            ammoBar = GetComponentInChildren<AmmoBar>();
            ammoBar.SetMaxAmmo(rangedMaxAmmo);
            ammoBar.SetAmmo(rangedMaxAmmo);
        }
        else basicEnemyPatrol = GetComponent<BasicEnemyPatrol>();

        if (basicEnemyPatrol != null)
        {
            if (basicEnemyPatrol.enemyType == BasicEnemyPatrol.EnemyType.Melee)
            {
                rangedMaxAmmo = 5;
            }

            if (basicEnemyPatrol.enemyType == BasicEnemyPatrol.EnemyType.Boss)
            {
                basicEnemyPatrol.player = GameObject.FindGameObjectWithTag("Player");
            }
        }

        if (chargerCollider != null) chargerCollider.SetActive(false);
        damageAoeTest = GetComponent<DamageAOETest>();
        rangedCurrentAmmo = rangedMaxAmmo;
        enemyLayer = LayerMask.GetMask("Enemies");
        playerLayer = LayerMask.GetMask("Player");
        health = GetComponent<Health>();
        canFirstCombo = false;
        canSecondCombo = false;
        falconDashCD = false;
    }

    private void OnDisable()
    {
        if (gameObject.CompareTag("Player"))
        {
            playerInputActions.Player.MeleeAttack.performed -= MeleeAttack;
            playerInputActions.Player.RangedAttack.performed -= RangedAttack;
        }
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<Movement>();
        currentlyAttacking = false;
        rechargingAmmo = false;
    }

    void Update()
    {
        meleeComboTimer += Time.deltaTime;
        
        //find a way to make this work for spider boss?
        if (gameObject.CompareTag("Player"))
        {
            if (meleeComboTimer > meleeComboTimerCutoff)
            {
                meleeComboTimer = 0;
                canFirstCombo = false;
                canSecondCombo = false;
                currentlyAttacking = false;
            }
        }
        
        ExitMeleeAttack();
    }

    public void EnemyAttack(BasicEnemyPatrol.EnemyType value, float aggroSpeed, bool playerInMeleeRange)
    {
        if (value == BasicEnemyPatrol.EnemyType.Melee)
        {
            EnemyMeleeAttack();
        }

        if (value == BasicEnemyPatrol.EnemyType.Ranged)
        {
            EnemyRangedAttack();
        }
        
        if (value == BasicEnemyPatrol.EnemyType.Charger)
        {
            EnemyChargerAttack();
            chargerAttackSpeed = aggroSpeed;
        }

        if (value == BasicEnemyPatrol.EnemyType.Boss)
        {
            EnemyBossAttack(playerInMeleeRange);
        }
    }

    private void EnemyMeleeAttack()
    {
        StartCoroutine(MeleeAttackCoroutine());
    }

    private void EnemyRangedAttack()
    {
        StartCoroutine(RangedAttackCoroutine());
        if (!rechargingAmmo)
        {
            StartCoroutine(RechargeAmmo());
        }
    }

    private void EnemyChargerAttack()
    {
        StartCoroutine(ChargerAttackCoroutine());
    }

    private void EnemyBossAttack(bool playerInMeleeRange)
    {
        if (playerInMeleeRange)
        {
            StartCoroutine(BossAttackCoroutine("Melee"));
        }
        else
        {
            int random = UnityEngine.Random.Range(0, 100);
            if (random < 90)
            {
                if (random < 30)
                {
                    StartCoroutine(BossAttackCoroutine("Web Blast"));
                }
                else if (random < 60)
                {
                    StartCoroutine(BossAttackCoroutine("Venom Spit"));
                }
                else
                {
                    StartCoroutine(BossAttackCoroutine("Charge"));
                }
            }
            else
            {
                StartCoroutine(BossAttackCoroutine("Spider Special Attack"));
            }
        }
    }

    public void SpiderAttackOverride()
    {
        StartCoroutine(BossAttackCoroutine("Spider Special Attack"));
    }

    private IEnumerator BossAttackCoroutine(string attackType)
    {
        if (!currentlyAttacking && attackType == "Melee")
        {
            currentlyAttacking = true;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            animator.SetTrigger("Melee");
            yield return new WaitForSeconds(meleeHitDelay);
        }

        if (!currentlyAttacking && attackType == "Web Blast")
        {
            currentlyAttacking = true;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            animator.SetTrigger("Web");
            yield return new WaitForSeconds(rangedHitDelay);
        }

        if (!currentlyAttacking && attackType == "Venom Spit")
        {
            currentlyAttacking = true;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            animator.SetTrigger("Venom");
            yield return new WaitForSeconds(rangedHitDelay);
        }

        if (!currentlyAttacking && attackType == "Charge")
        {
            StartCoroutine(ChargerAttackCoroutine());
            animator.SetTrigger("Charge");
            yield return new WaitForSeconds(meleeHitDelay);
        }

        if (!currentlyAttacking && attackType == "Spider Special Attack")
        {
            currentlyAttacking = true;
            basicEnemyPatrol.playerHiding = 0;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            animator.SetTrigger("Special");
            
            //go up, replace with function that's called via animation event
            StartCoroutine(ClimbUp());

            //wait for a bit before aligning with player
            yield return new WaitForSeconds(specialAttackHitDelay/2);
            
            //move to above player
            transform.position = new Vector3(basicEnemyPatrol.player.transform.position.x,transform.position.y,0);
            basicEnemyPatrol.playerHiding = 0;
            yield return new WaitForSeconds(0.01f);

            StartCoroutine(CrashDown());
            basicEnemyPatrol.playerHiding = 0;
        }
    }

    public void SetBossProjectileAngle()
    {
        if (basicEnemyPatrol.enemyType == BasicEnemyPatrol.EnemyType.Boss)
        {
            projectileSpeed.y = basicEnemyPatrol.targetDirRaw.y;
        }
    }

    private IEnumerator ClimbUp()
    {
        spiderBossTileCollider.SetActive(false);
        Vector3 goal = new Vector3(transform.position.x, transform.position.y + 20, 0);
        float rateOfMovement = climbUpSpeed;
        while (true)
        {
            Vector3 start = transform.position;
            if (start == goal) break;
            transform.position = Vector3.MoveTowards(start, goal, Time.deltaTime * rateOfMovement);
            yield return new WaitForEndOfFrame();
        }
        spiderBossTileCollider.SetActive(true);
    }

    private IEnumerator CrashDown()
    {
        crashDownCollider.SetActive(true);
        int tempDamage = damageAoeTest.damageRate;
        damageAoeTest.damageRate = 0;
        Vector3 goal = new Vector3(transform.position.x, -0.07f, 0);
        float rateOfMovement = crashDownSpeed;
        while (true)
        {
            Vector3 start = transform.position;
            if (start == goal) break;
            transform.position = Vector3.MoveTowards(start, goal, Time.deltaTime * rateOfMovement);
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(MoveAfterCrash());
        crashDownCollider.SetActive(false);
        damageAoeTest.damageRate = tempDamage;
    }

    private IEnumerator MoveAfterCrash()
    {
        yield return new WaitForSeconds(1f);
        currentlyAttacking = false;
        basicEnemyPatrol.BossEndAttackAnimation();
    }

    #region Melee

    public void MeleeAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //StartCoroutine(MeleeAttackCoroutine());
            MeleeAttackComboNew();
        }
    }

    private void MeleeAttackComboNew()
    {
        if (Time.time - lastComboEnd > whatDoICallThis && comboCounter < meleeCombo.Count)
        {
            CancelInvoke("EndMeleeCombo");
            currentlyAttacking = true;

            if (Time.time - lastClickedTime >= internalComboTimeDelay)
            {
                
                animator.runtimeAnimatorController = meleeCombo[comboCounter].animatorOV;
                animator.Play("MeleeAttack",0,0);
                lastClickedTime = Time.time;
                
                if(comboCounter<meleeCombo.Count) comboCounter++;
                else Invoke("EndMeleeCombo", 0);
            }
        }
        
        else if ((Time.time - lastComboEnd > whatDoICallThis && comboCounter >= meleeCombo.Count))
        {
            if (Time.time - lastClickedTime >= internalComboTimeDelay)
            {
                lastClickedTime = Time.time;
                if(comboCounter<meleeCombo.Count-1) comboCounter++;
                else Invoke("EndMeleeCombo", 0.4f);
            }
        }
        
        /*else if (Time.time - lastComboEnd > whatDoICallThis && comboCounter < meleeCombo.Count &&
                 !animator.GetCurrentAnimatorStateInfo(0).IsTag("MeleeAttack"))
        {
            if (Time.time - lastClickedTime >= internalComboTimeDelay)
            {
                animator.runtimeAnimatorController = meleeCombo[comboCounter].animatorOV;
                animator.Play("MeleeAttack",0,0);
                lastClickedTime = Time.time;
                
                if(comboCounter<meleeCombo.Count-1) comboCounter++;
                else Invoke("EndMeleeCombo", 0);
            }
        }*/
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        if (!currentlyAttacking && !canFirstCombo && !canSecondCombo)
        {
            meleeComboTimer = 0;
            currentlyAttacking = true;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            if (gameObject.CompareTag("Player"))
            {
                //animator.SetTrigger("MeleeAttack");
                canFirstCombo = true;
                //yield return new WaitForSeconds(meleeHitDelay);
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                //canFirstCombo = true;
            }

            if (gameObject.CompareTag("Enemy"))
            {
                animator.SetTrigger("Attack");
                yield return new WaitForSeconds(meleeHitDelay);
                
                //Detect enemies in range of attack
                Collider2D hitEnemy = Physics2D.OverlapCircle(meleeAttackPoint.position, meleeAttackRange, playerLayer);

                //Damage them
                if (hitEnemy != null)
                {
                    hitEnemy.GetComponentInParent<Health>().ChangeHealth(-meleeAttackPower,this.gameObject);
                }
            }
        }
        else if (!currentlyAttacking && canFirstCombo && !canSecondCombo)
        {
            meleeComboTimer = 0;
            currentlyAttacking = true;
            canFirstCombo = false;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            if (gameObject.CompareTag("Player"))
            {
                canSecondCombo = true;
                //animator.SetTrigger("MeleeAttack2");
                //yield return new WaitForSeconds(meleeHitDelay);
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                //canSecondCombo = true;
            }
        }
        else if (!currentlyAttacking && !canFirstCombo && canSecondCombo)
        {
            meleeComboTimer = 0;
            currentlyAttacking = true;
            canSecondCombo = false;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            if (gameObject.CompareTag("Player"))
            {
                //animator.SetTrigger("MeleeAttack3");
                //yield return new WaitForSeconds(meleeHitDelay);
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            }
        }
    }

    void ExitMeleeAttack()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f &&
            animator.GetCurrentAnimatorStateInfo(0).IsTag("MeleeAttack"))
        {
            Invoke("EndMeleeCombo", externalComboTimeDelay);
            //print("invoke end combo");
        }
    }

    void EndMeleeCombo()
    {
        comboCounter = 0;
        lastComboEnd = Time.time;
        currentlyAttacking = false;
        //print("resetting lastComboEnd");
    }

    private IEnumerator MeleeAttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        currentlyAttacking = false;
    }

    private IEnumerator ChargerAttackCoroutine()
    {
        if (!currentlyAttacking)
        {
            currentlyAttacking = true;
            StartCoroutine(MeleeAttackCooldownCoroutine());
            int tempDamage = damageAoeTest.damageRate;
            damageAoeTest.damageRate = 0;
            if (basicEnemyPatrol.enemyType == BasicEnemyPatrol.EnemyType.Boss) basicEnemyPatrol.bossCharging = true;
            if(gameObject.CompareTag("Enemy") && basicEnemyPatrol.enemyType == BasicEnemyPatrol.EnemyType.Charger) animator.SetBool("Charge", true);
            yield return new WaitForSeconds(0.2f);
            if (chargerCollider != null) chargerCollider.SetActive(true);
            yield return new WaitForSeconds(meleeHitDelay);
            if (chargerCollider != null) chargerCollider.SetActive(false);
            damageAoeTest.damageRate = tempDamage;
            yield return new WaitForSeconds(chargeTransitionDelay);
            if (basicEnemyPatrol.enemyType == BasicEnemyPatrol.EnemyType.Boss)
            {
                animator.SetTrigger("FinishCharge");
                print("finish charge");
                basicEnemyPatrol.bossCharging = false;
                basicEnemyPatrol.BossEndAttackAnimation();
            }
            if(gameObject.CompareTag("Enemy") && basicEnemyPatrol.enemyType == BasicEnemyPatrol.EnemyType.Charger) animator.SetBool("Charge", false);
            
        }
    }

    #endregion

    #region Ranged

    public void RangedAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (falconMask)
            {
                StartCoroutine(FalconDashAttack());
            }
            else if (spiderMask)
            {
                
            }
            else
            {
                StartCoroutine(RangedAttackCoroutine());
            }
            
            if (!rechargingAmmo)
            {
                StartCoroutine(RechargeAmmo());
            }
        }
    }

    private IEnumerator RangedAttackCoroutine()
    {
        if (!currentlyAttacking && rangedCurrentAmmo > 0)
        {
            lastComboEnd = Time.time;
            currentlyAttacking = true;
            rangedCurrentAmmo -= 1;
            StartCoroutine(RangedAttackCooldownCoroutine());
            if(gameObject.CompareTag("Player")) animator.SetTrigger("RangedAttack");
            if(gameObject.CompareTag("Enemy")) animator.SetTrigger("Attack");

            yield return new WaitForSeconds(rangedHitDelay);
            lastComboEnd = Time.time;
            //if (health.currentHealth > 0) FireProjectile();
        }
    }

    private IEnumerator FalconDashAttack()
    {
        // Making sure we aren't attacking or dashing before we start another dash
        if (!currentlyAttacking || !falconDashCD )
        {
            if (movement.isTouchingGround)
            {
                
                falconDashCD = true;
                lastComboEnd = Time.time;
                currentlyAttacking = true;
                // activating the animation for the dash
                if (gameObject.CompareTag("Player"))
                {
                    animator.SetBool("FalconDash", true);
                    animator.Play("falconDashEnter_ANIM");
                }

                
                // waiting for the start up animation to finish before actually dashing
                yield return new WaitForSeconds(0.2f);
                // calling the movement dash to start the physics of the dash
                health.immune = true;
                movement.FalconDash();
                
                //falconDashCollider.SetActive(true);
                
                yield return new WaitForSeconds(rangedHitDelay);
                
                //falconDashCollider.SetActive(false);
                health.immune = false;
                lastComboEnd = Time.time;
            }
        }
    }
    
    public void FireProjectile(string projectileType)
    {
        if (gameObject.CompareTag("Player")) ammoBar.SetAmmo(rangedCurrentAmmo);
        if (basicEnemyPatrol.enemyType == BasicEnemyPatrol.EnemyType.Boss)
        {
            if (projectileType == "Web") defaultProjectilePrefab = webBlastProjectile;
            if (projectileType == "Venom") defaultProjectilePrefab = venomSpitProjectile;
        }
        GameObject go = Instantiate(defaultProjectilePrefab, launchPoint.position, defaultProjectilePrefab.transform.rotation);
        go.GetComponent<Projectile>()
            .SetProjectileValues(flightTime, this.gameObject, projectileSpeed, rangedAttackPower);
        Vector3 origin = go.transform.localScale;
        go.transform.localScale = new Vector3(origin.x * transform.localScale.x, origin.y, origin.z);
    }
    
    private IEnumerator RangedAttackCooldownCoroutine()
    {
        //print("ranged attack cd cr started");
        yield return new WaitForSeconds(attackCooldown);
        
        currentlyAttacking = false;
    }

    
    //if we add something that forcibly changes the ammo value (eg, a powerup/pickup)
    //we'll need to manually stop this coroutine most likely
    private IEnumerator RechargeAmmo()
    {
        rechargingAmmo = true;
        yield return new WaitForSeconds(rechargeAmmoTime);
        if (rangedCurrentAmmo < rangedMaxAmmo)
        {
            rangedCurrentAmmo++;
            StartCoroutine(RechargeAmmo());
            if (gameObject.CompareTag("Player")) ammoBar.SetAmmo(rangedCurrentAmmo);
        }
        else if (rangedCurrentAmmo == rangedMaxAmmo)
        {
            rechargingAmmo = false;
            if (gameObject.CompareTag("Player")) ammoBar.SetAmmo(rangedCurrentAmmo);
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRange);
    }
}
