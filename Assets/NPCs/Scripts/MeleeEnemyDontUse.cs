using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyDontUse : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rigidbody;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if ((rigidbody.velocity.x > 0 || rigidbody.velocity.x < 0))
        {
            animator.SetBool("Running", true);
        }
        else
        {
            animator.SetBool("Running", false);
        }
    }
}
