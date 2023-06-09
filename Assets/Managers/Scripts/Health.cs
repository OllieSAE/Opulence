using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float currentHealth;

    public float maxHealth;
    
    private Animator animator;

    public delegate void DeathEvent(GameObject parent);
    public event DeathEvent deathEvent;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("Dead", false);
    }

    public void ChangeHealth(float amount, GameObject whoDealtDamage)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Death(this.gameObject);
        }
    }

    public void Death(GameObject go)
    {
        animator.SetBool("Dead",true);
        deathEvent?.Invoke(go);
    }
}
