using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    private PlayerInput playerInput;
    private PlayerInputActions playerInputActions;
    private Vector2 inputVector;

    public Animator animator;

    public Transform groundCheck;
    public Transform wallCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    private bool isTouchingGround;

    //delete once L/R sprites exist
    private bool facingLeft;

    private bool dashing;
    private bool doubleJump;
    private bool midairDash;
    
    public float speed;
    public float jumpValue;
    public float dashValue;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Jump.canceled += ArrestJump;
        //playerInputActions.Player.Movement.performed += Move;
        playerInputActions.Player.Dash.performed += Dash;
        playerInputActions.Player.Crouch.performed += Crouch;

        dashing = false;
        doubleJump = false;
        midairDash = false;
    }

    private void Update()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        isTouchingGround = Physics2D.OverlapCircle(groundCheck.position,groundCheckRadius, groundLayer);
        if (isTouchingGround)
        {
            midairDash = true;
            
        }

        //TODO: fix the animations to work properly
        // need back outs/exits that work appropriately
        // play around with paper prototype maybe
        
        
        animator.SetFloat("Y velocity", rigidbody.velocity.y);
        
        if ((rigidbody.velocity.x > 0 || rigidbody.velocity.x < 0) && isTouchingGround)
        {
            animator.SetBool("Running", true);
        }
        else animator.SetBool("Running", false);
    }

    private void FixedUpdate()
    {
        if (!dashing)
        {
            rigidbody.velocity = new Vector2(inputVector.x * speed * Time.fixedDeltaTime, rigidbody.velocity.y);
        }
        
        
        //remove this when we have L/R sprites
        if (inputVector.x > 0 && facingLeft)
        {
            Flip();
        }
        else if (inputVector.x < 0 && !facingLeft)
        {
            Flip();
        }
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isTouchingGround && doubleJump && !dashing)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
                rigidbody.AddForce(Vector3.up * jumpValue, ForceMode2D.Impulse);
                doubleJump = false;
            }
            else if (isTouchingGround)
            {
                rigidbody.AddForce(Vector3.up * jumpValue, ForceMode2D.Impulse);
                doubleJump = true;
            }
        }
    }

    public void StartMove(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //StartCoroutine();
        }
    }

    public void StopMove(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            //StopCoroutine();
        }
    }

    public void ArrestJump(InputAction.CallbackContext context)
    {
        if(rigidbody.velocity.y > 0) rigidbody.velocity = new Vector2(rigidbody.velocity.x,0);
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isTouchingGround && midairDash)
            {
                midairDash = false;
                StartCoroutine(DashingCoroutine());
                animator.SetBool("Dashing", true);
                if (!facingLeft)
                {
                    rigidbody.AddForce(Vector3.right * dashValue, ForceMode2D.Impulse);
                }

                if (facingLeft)
                {
                    rigidbody.AddForce(Vector3.left * dashValue, ForceMode2D.Impulse);
                }
            }
            else if (isTouchingGround)
            {
                StartCoroutine(DashingCoroutine());
                animator.SetBool("Dashing", true);
                if (!facingLeft)
                {
                    rigidbody.AddForce(Vector3.right * dashValue, ForceMode2D.Impulse);
                }

                if (facingLeft)
                {
                    rigidbody.AddForce(Vector3.left * dashValue, ForceMode2D.Impulse);
                }
            }
        }
    }

    private IEnumerator DashingCoroutine()
    {
        dashing = true;
        
        rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        yield return new WaitForSeconds(0.25f);
        dashing = false;
        animator.SetBool("Dashing", false);
        rigidbody.constraints = RigidbodyConstraints2D.None;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if(context.performed) ;
    }

    //Temporary fix for placeholder sprites
    //will replace with actual L/R sprites
    public void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
