using System.Collections;
using System.Collections.Generic;
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
    private bool facingLeft;
    
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        isGroundAhead = Physics2D.OverlapCircle(groundAheadCheck.position,groundCheckRadius, groundLayer);
        isWallAhead = Physics2D.OverlapCircle(wallAheadCheck.position,wallCheckRadius, groundLayer);
        Patrol();
    }

    void Patrol()
    {
        if (isGroundAhead && !facingLeft && !isWallAhead)
        {
            transform.Translate(Vector3.right * Time.deltaTime);
        }
        else if (isGroundAhead && facingLeft && !isWallAhead)
        {
            transform.Translate(Vector3.left * Time.deltaTime);
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
