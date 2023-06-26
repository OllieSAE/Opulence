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

    public delegate void DeathEvent(GameObject parent);
    public event DeathEvent deathEvent;
    
    private void Awake()
    {
        if (GetComponentInChildren<HealthBar>() != null)
        {
            healthBar = GetComponentInChildren<HealthBar>();
            thisIsPlayer = true;
        }

        
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("Dead", false);
        if(healthBar!=null) healthBar.SetMaxHealth(maxHealth);
        
        GameManager.Instance.SubscribeToDeathEvents(this);
    }

    public void ChangeHealth(int amount, GameObject whoDealtDamage)
    {
        currentHealth += amount;
        
        if (healthBar != null) healthBar.SetHealth(currentHealth);
        
        if (amount < 0 && currentHealth > 0 && thisIsPlayer)
        {
            //check who dealt damage, play appropriate sound
            RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Damage");
            
        }
        else if (amount < 0 && currentHealth >= 0 && !thisIsPlayer)
        {
            //RuntimeManager.PlayOneShot("enemy takes damage sound")
            
            StartCoroutine(EnemyDamagedCoroutine());
        }
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (currentHealth <= 0)
        {
            currentHealth = -1;
            StartCoroutine(Death(this.gameObject));
        }
    }

    private IEnumerator EnemyDamagedCoroutine()
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
            deathCR = true;
            animator.SetBool("Dead",true);
            deathEvent?.Invoke(go);
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
