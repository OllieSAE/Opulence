using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float currentHealth;
    public bool isDead;

    public float maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
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
            isDead = true;
        }
    }
}
