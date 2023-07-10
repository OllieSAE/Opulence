using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Combat : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    private Animator animator;
    private DamageAOETest damageAoeTest;
    
    [Header("General Combat")]
    private bool currentlyAttacking;
    public float attackCooldown;
    private LayerMask enemyLayer;
    private LayerMask playerLayer;
    private Health health;
    //private BasicEnemyPatrol basicEnemyPatrol;

    [Header("Melee Combat")]
    public int meleeAttackPower;
    public Transform meleeAttackPoint;
    public float meleeAttackRange;
    public float meleeHitDelay;
    public float chargeTransitionDelay;

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
    public GameObject projectilePrefab;
    public Vector2 projectileSpeed;
    private bool rechargingAmmo;

    [Header("Charger Enemy Combat")]
    public GameObject chargerCollider;

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
        //else basicEnemyPatrol = GetComponent<BasicEnemyPatrol>();

        if (chargerCollider != null) chargerCollider.SetActive(false);
        damageAoeTest = GetComponent<DamageAOETest>();
        rangedCurrentAmmo = rangedMaxAmmo;
        enemyLayer = LayerMask.GetMask("Enemies");
        playerLayer = LayerMask.GetMask("Player");
        health = GetComponent<Health>();
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
        currentlyAttacking = false;
        rechargingAmmo = false;
    }

    public void EnemyAttack(BasicEnemyPatrol.EnemyType value, float aggroSpeed)
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
            EnemyBossAttack();
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

    private void EnemyBossAttack()
    {
        print("boss attack");
    }

    #region Melee

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
            if (gameObject.CompareTag("Player"))
            {
                animator.SetTrigger("MeleeAttack");
                yield return new WaitForSeconds(meleeHitDelay);
                //Detect enemies in range of attack
                
                //change this to overlapBOXall
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(meleeAttackPoint.position, meleeAttackRange, enemyLayer);
                
                //can we set the "attack point" and "attack range" variables to default at the start
                //then adjust them at the beginning of each attack animation, via event

                //Damage them
                foreach (Collider2D enemy in hitEnemies)
                {
                    enemy.GetComponentInParent<Health>().ChangeHealth(-meleeAttackPower,this.gameObject);
                }
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
            //if (gameObject.CompareTag("Player")) ;
            if(gameObject.CompareTag("Enemy")) animator.SetBool("Charge", true);
            if (chargerCollider != null) chargerCollider.SetActive(true);
            int tempDamage = damageAoeTest.damageRate;
            damageAoeTest.damageRate = 0;
            yield return new WaitForSeconds(meleeHitDelay);
            if (chargerCollider != null) chargerCollider.SetActive(false);
            damageAoeTest.damageRate = tempDamage;
            yield return new WaitForSeconds(chargeTransitionDelay);
            if(gameObject.CompareTag("Enemy")) animator.SetBool("Charge", false);
            
        }
    }

    #endregion

    #region Ranged

    public void RangedAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartCoroutine(RangedAttackCoroutine());
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
            
            currentlyAttacking = true;
            rangedCurrentAmmo -= 1;
            StartCoroutine(RangedAttackCooldownCoroutine());
            if(gameObject.CompareTag("Player")) animator.SetTrigger("RangedAttack");
            if(gameObject.CompareTag("Enemy")) animator.SetTrigger("Attack");

            yield return new WaitForSeconds(rangedHitDelay);
            
            if (health.currentHealth > 0) FireProjectile();
        }
    }
    
    public void FireProjectile()
    {
        if (gameObject.CompareTag("Player")) ammoBar.SetAmmo(rangedCurrentAmmo);
        GameObject go = Instantiate(projectilePrefab, launchPoint.position, projectilePrefab.transform.rotation);
        go.GetComponent<Projectile>()
            .SetProjectileValues(flightTime, this.gameObject, projectileSpeed, rangedAttackPower);
        Vector3 origin = go.transform.localScale;
        go.transform.localScale = new Vector3(origin.x * transform.localScale.x, origin.y, origin.z);
    }
    
    private IEnumerator RangedAttackCooldownCoroutine()
    {
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
