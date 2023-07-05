using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int currentHealth;

    public int maxHealth;

    private Animator animator;
    private bool thisIsPlayer = false;
    private bool deathCR = false;
    public HealthBar healthBar;
    public LayerMask playerLayer;
    private SpriteFlash flashEffect;

    public delegate void DeathEvent(GameObject parent);
    public event DeathEvent deathEvent;
    
    private void Awake()
    {
        if (GetComponentInChildren<HealthBar>() != null)
        {
            healthBar = GetComponentInChildren<HealthBar>();
            thisIsPlayer = true;
        }

        
        if (thisIsPlayer)
        {
            flashEffect = GetComponentInChildren<SpriteFlash>();
        }
        else flashEffect = GetComponent<SpriteFlash>();
    
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("Dead", false);
        if(healthBar!=null) healthBar.SetMaxHealth(maxHealth);
    }

    private void Start()
    {
        GameManager.Instance.SubscribeToDeathEvents(this);
    }

    public void ChangeHealth(int amount, GameObject whoDealtDamage)
    {
        currentHealth += amount;
        
        if (healthBar != null) healthBar.SetHealth(currentHealth);
        
        if (amount < 0 && currentHealth > 0 && thisIsPlayer)
        {
            flashEffect.Flash();
            if (whoDealtDamage.CompareTag("Hazard"))
            {
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Damage");
            }
            else if (whoDealtDamage.CompareTag("Enemy"))
            {
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Placeholder");
            }
            
        }
        else if (amount < 0 && currentHealth >= 0 && !thisIsPlayer)
        {
            //RuntimeManager.PlayOneShot("enemy takes damage sound")
            flashEffect.Flash();
            //StartCoroutine(EnemyDamagedCoroutine());
        }
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (currentHealth <= 0)
        {
            currentHealth = -1;
            StartCoroutine(Death(this.gameObject));
            
            //combat tutorial only
            if(CombatTestManager.Instance != null) CombatTestManager.Instance.KilledEnemy(this.gameObject);
        }
    }

    private IEnumerator EnemyDamagedShrinkCoroutine()
    {
        Vector3 scale = transform.localScale;
        float xScale = scale.x;
        float yScale = scale.y;
        float zScale = scale.z;
        
        transform.localScale = new Vector3(xScale * 0.75f, yScale * 0.75f, zScale);
        yield return new WaitForSeconds(0.2f);
        transform.localScale = scale;
    }

    public IEnumerator Death(GameObject go)
    {
        if (!deathCR)
        {
            flashEffect.Flash();
            deathCR = true;
            animator.SetBool("Dead",true);
            deathEvent?.Invoke(go);
            if (!thisIsPlayer)
            {
                GetComponent<Combat>().enabled = false;
                GetComponent<BoxCollider2D>().enabled = false;
            }
            if(thisIsPlayer) RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Death");
            yield return new WaitForSeconds(5f);
            deathCR = false;
        }

        if (!thisIsPlayer)
        {
            Destroy(this.gameObject);
        }
        
        
    }
}
