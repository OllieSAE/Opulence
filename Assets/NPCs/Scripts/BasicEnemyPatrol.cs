using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicEnemyPatrol : MonoBehaviour
{
    public Transform groundAheadCheck;
    public Transform wallAheadCheck;
    public float groundCheckRadius;
    public float wallCheckRadius;
    public LayerMask groundLayer;
    public bool isGroundAhead;
    public bool isWallAhead;
    private Rigidbody2D rigidbody;
    private Animator animator;
    private bool facingLeft;
    public bool patrolling = false;
    
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        GameManager.Instance.enableEnemyPatrolEvent += EnablePatrolling;
        GameManager.Instance.disableEnemyPatrolEvent += DisablePatrolling;
    }

    private void OnDisable()
    {
        GameManager.Instance.enableEnemyPatrolEvent -= EnablePatrolling;
        GameManager.Instance.disableEnemyPatrolEvent -= DisablePatrolling;
    }

    void EnablePatrolling()
    {
        patrolling = true;
    }

    void DisablePatrolling()
    {
        patrolling = false;
        animator.SetBool("Running", false);
    }

    void Update()
    {
        isGroundAhead = Physics2D.OverlapCircle(groundAheadCheck.position,groundCheckRadius, groundLayer);
        isWallAhead = Physics2D.OverlapCircle(wallAheadCheck.position,wallCheckRadius, groundLayer);
        if(patrolling) Patrol();
    }

    void Patrol()
    {
        if (isGroundAhead && !facingLeft && !isWallAhead)
        {
            transform.Translate(Vector3.right * Time.deltaTime);
            animator.SetBool("Running", true);
        }
        else if (isGroundAhead && facingLeft && !isWallAhead)
        {
            transform.Translate(Vector3.left * Time.deltaTime);
            animator.SetBool("Running", true);
        }
        else if (!isGroundAhead || isWallAhead)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
