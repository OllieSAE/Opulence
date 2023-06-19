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
    public HealthBar healthBar;

    public delegate void DeathEvent(GameObject parent);
    public event DeathEvent deathEvent;
    
    private void Awake()
    {
        if (GetComponentInChildren<HealthBar>() != null)
        {
            healthBar = GetComponentInChildren<HealthBar>();
        }
        
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("Dead", false);
        healthBar.SetMaxHealth(maxHealth);
    }

    public void ChangeHealth(int amount, GameObject whoDealtDamage)
    {
        currentHealth += amount;
        healthBar.SetHealth(currentHealth);
        if (amount < 0 && currentHealth >= 0)
        {
            RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Jump");
            print(this.gameObject + " took damage from " +whoDealtDamage);
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
