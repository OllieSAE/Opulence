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
    }

    public void ChangeHealth(int amount, GameObject whoDealtDamage)
    {
        currentHealth += amount;
        
        if (healthBar != null) healthBar.SetHealth(currentHealth);
        
        if (amount < 0 && currentHealth >= 0 && thisIsPlayer)
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
        //this is fucking the -1 flip direction
        //take it's current X value and multiply by 0.75 (since it's either 1 or -1)
        transform.localScale = new Vector3(0.75f, 0.75f, 1);
        yield return new WaitForSeconds(0.2f);
        transform.localScale = Vector3.one;
    }

    public IEnumerator Death(GameObject go)
    {
        if (!deathCR)
        {
            print("player respawn coroutine started");
            deathCR = true;
            animator.SetBool("Dead",true);
            deathEvent?.Invoke(go);
            if(thisIsPlayer) RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Death");
            yield return new WaitForSeconds(5f);
            deathCR = false;
        }
        
    }
}
