using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DamageAOETest : MonoBehaviour
{
    public int damageRate;
    public float timer;

    private List<Health> availableTargets;

    private void Start()
    {
        availableTargets = new List<Health>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (availableTargets.Count > 0 && timer >= 1)
        {
            timer = 0;
            DealDamage();
        }
    }

    private void DealDamage()
    {
        foreach (Health target in availableTargets)
        {
            if (target.gameObject.CompareTag("Player"))
            {
                target.ChangeHealth(-damageRate,this.GameObject());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Health>() != null)
        {
            Health tempTarget = other.GetComponentInParent<Health>();
            if(!availableTargets.Contains(tempTarget))
            {
                availableTargets.Add(tempTarget);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<Health>() != null)
        {
            Health tempTarget = other.GetComponentInParent<Health>();
            if(availableTargets.Contains(tempTarget))
            {
                availableTargets.Remove(tempTarget);
            }
        }
    }
}
