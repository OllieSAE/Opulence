using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    private PlayerInput playerInput;
    private PlayerInputActions playerInputActions;
    private Vector2 inputVector;

    public Animator animator;
    private Health health;

    public Transform groundCheck;
    public Transform wallCheck;
    public float groundCheckRadius;
    public float wallCheckRadius;
    public LayerMask groundLayer;
    public bool isTouchingGround;
    public bool isTouchingWall;

    //delete once L/R sprites exist
    private bool facingLeft;

    private bool dashing;
    private bool doubleJump;
    private bool midairDash;
    public bool isSliding;
    private bool wallJumping;
    private bool inputAllowed;
    
    public float speed;
    public float jumpValue;
    public float dashValue;
    public float wallSlidingSpeed;
    public float wallJumpDuration;
    public float controlLockDuration;
    public Vector2 wallJumpForce;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        health = GetComponent<Health>();

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
        wallJumping = false;
        inputAllowed = true;
    }

    private void OnEnable()
    {
        
    }

    private void Start()
    {
        GameManager.Instance.playerRespawnEvent += Respawn;
    }

    private void OnDisable()
    {
        GameManager.Instance.playerRespawnEvent -= Respawn;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawLine(groundCheck.position,new Vector3(groundCheck.position.x,groundCheck.position.y - groundCheckRadius,groundCheck.position.z));
            if (!isTouchingGround) Gizmos.color = Color.red;
            else Gizmos.color = Color.green;
        }

        if (wallCheck != null && facingLeft)
        {
            Gizmos.DrawLine(wallCheck.position,new Vector3(wallCheck.position.x - wallCheckRadius,wallCheck.position.y,wallCheck.position.z));
            if (!isTouchingWall) Gizmos.color = Color.red;
            else Gizmos.color = Color.green;
        }
        
        if (wallCheck != null && !facingLeft)
        {
            Gizmos.DrawLine(wallCheck.position,new Vector3(wallCheck.position.x + wallCheckRadius,wallCheck.position.y,wallCheck.position.z));
            if (!isTouchingWall) Gizmos.color = Color.red;
            else Gizmos.color = Color.green;
        }
    }

    private void Update()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        isTouchingGround = Physics2D.OverlapCircle(groundCheck.position,groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position,wallCheckRadius, groundLayer);
        if (isTouchingGround)
        {
            midairDash = true;
        }

        if (!isTouchingGround && isTouchingWall)
        {
            isSliding = true;
        }
        else isSliding = false;

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
        if (isSliding)
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x,
                Mathf.Clamp(rigidbody.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }

        if (wallJumping)
        {
            inputAllowed = false;
            StartCoroutine(WallJumpDelayCoroutine());
            if (facingLeft)
            {
                rigidbody.AddForce(wallJumpForce,ForceMode2D.Impulse);
            }

            if (!facingLeft)
            {
                rigidbody.AddForce(new Vector2(-wallJumpForce.x,wallJumpForce.y),ForceMode2D.Impulse);
            }
        }

        else if (!dashing && !isSliding)
        {
            if (inputAllowed) rigidbody.velocity = new Vector2(inputVector.x * speed * Time.fixedDeltaTime, rigidbody.velocity.y);
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

    public void Respawn()
    {
        print("player movement tried to respawn");
        animator.SetBool("Dead", false);
        health.currentHealth = health.maxHealth;
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isTouchingGround && !isTouchingWall && doubleJump && !dashing)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
                rigidbody.AddForce(Vector3.up * jumpValue, ForceMode2D.Impulse);
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Dash");
                doubleJump = false;
            }
            else if (isTouchingGround)
            {
                rigidbody.AddForce(Vector3.up * jumpValue, ForceMode2D.Impulse);
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Dash");
                doubleJump = true;
            }
            else if (isSliding)
            {
                wallJumping = true;
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Dash");
                Invoke("StopWallJump", wallJumpDuration);
            }
        }
    }

    void StopWallJump()
    {
        wallJumping = false;
    }

    public IEnumerator WallJumpDelayCoroutine()
    {
        inputAllowed = false;
        print("controls locked");
        yield return new WaitForSeconds(controlLockDuration);
        print("controls unlocked");
        inputAllowed = true;
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

    //TODO: Add invulnerability to Dash, maybe with a cooldown too
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
