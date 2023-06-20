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
    public HealthBar healthBar;
    public LayerMask playerLayer;

    public delegate void DeathEvent(GameObject parent);
    public event DeathEvent deathEvent;
    
    private void Awake()
    {
        if (GetComponentInChildren<HealthBar>() != null)
        {
            healthBar = GetComponentInChildren<HealthBar>();
        }

        if (gameObject.layer == playerLayer)
        {
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
            RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Jump");
            print(this.gameObject + " took damage from " +whoDealtDamage);
        }
        else if (amount < 0 && currentHealth >= 0 && !thisIsPlayer)
        {
            //RuntimeManager.PlayOneShot("enemy takes damage sound")
            print(gameObject.name + " took " + amount + " damage from Player!");
        }
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (currentHealth <= 0)
        {
            currentHealth = -1;
            Death(this.gameObject);
        }
    }

    public void Death(GameObject go)
    {
        animator.SetBool("Dead",true);
        deathEvent?.Invoke(go);
    }
}
