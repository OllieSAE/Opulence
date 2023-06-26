using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Combat : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    private Animator animator;
    
    [Header("General Combat")]
    private bool currentlyAttacking;
    public float attackCooldown;
    public LayerMask enemyLayers;
    
    [Header("Melee Combat")]
    public int meleeAttackPower;
    public Transform meleeAttackPoint;
    public float meleeAttackRange;
    public float meleeHitDelay;
    
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

    private void Awake()
    {
        if (gameObject.CompareTag("Player"))
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.Enable();
            playerInputActions.Player.MeleeAttack.performed += MeleeAttack;
            playerInputActions.Player.RangedAttack.performed += RangedAttack;
            rangedCurrentAmmo = rangedMaxAmmo;
            ammoBar = GetComponentInChildren<AmmoBar>();
            ammoBar.SetMaxAmmo(rangedMaxAmmo);
            ammoBar.SetAmmo(rangedMaxAmmo);
        }
    }

    private void OnDisable()
    {
        playerInputActions.Player.MeleeAttack.performed -= MeleeAttack;
        playerInputActions.Player.RangedAttack.performed -= RangedAttack;
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        currentlyAttacking = false;
        rechargingAmmo = false;
    }
    
    void Update()
    {
        //enemy AI stuff
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
            animator.SetTrigger("MeleeAttack");

            yield return new WaitForSeconds(meleeHitDelay);
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
            animator.SetTrigger("RangedAttack");

            yield return new WaitForSeconds(rangedHitDelay);
            
            FireProjectile();
            
        }
    }
    
    public void FireProjectile()
    {
        ammoBar.SetAmmo(rangedCurrentAmmo);
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
            ammoBar.SetAmmo(rangedCurrentAmmo);
        }
        else if (rangedCurrentAmmo == rangedMaxAmmo)
        {
            rechargingAmmo = false;
            ammoBar.SetAmmo(rangedCurrentAmmo);
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRange);
    }
}
