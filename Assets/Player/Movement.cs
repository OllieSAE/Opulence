using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class Movement : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    private PlayerInput playerInput;
    private PlayerInputActions playerInputActions;
    private Vector2 inputVector;
    private Health health;
    [Header("Animation")]
    public Animator animator;
    
    [Header("Environment Checks")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float groundCheckRadius;
    public float wallCheckRadius;
    public LayerMask groundLayer;
    public bool isTouchingGround;
    public bool isTouchingWall;

    private Vector3 playerRespawnPos;
    private bool respawnCR;

    //delete once L/R sprites exist
    private bool facingLeft;

    private bool dashing;
    private bool doubleJump;
    private bool midairDash;
    private bool isSliding;
    private bool wallJumping;
    private bool controlsSet;
    public bool inputAllowed;
    private bool isDead;
    
    [Header("Movement Values")]
    public float speed;
    public float jumpValue;
    public float dashValue;
    public float wallSlidingSpeed;
    public float wallJumpDuration;
    public float controlLockDuration;
    public Vector2 wallJumpForce;
    
    private FMOD.Studio.EventInstance playerWalk;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        health = GetComponent<Health>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Jump.canceled += ArrestJump;
        playerInputActions.Player.Dash.performed += Dash;
        playerInputActions.Player.Crouch.performed += Crouch;

        controlsSet = false;
        dashing = false;
        doubleJump = false;
        midairDash = false;
        wallJumping = false;
        respawnCR = false;
        isDead = false;
        playerRespawnPos = transform.position;

        playerWalk = RuntimeManager.CreateInstance("event:/SOUND EVENTS/Footsteps");
        RuntimeManager.AttachInstanceToGameObject(playerWalk, transform, rigidbody);
    }

    private void OnEnable()
    {
        
    }

    private void Start()
    {
        if (GameManager.Instance.tutorialTestEnable)
        {
            inputAllowed = false;
        }
        else inputAllowed = true;
        GameManager.Instance.playerRespawnEvent += Respawn;
        GameManager.Instance.tutorialDialogueFinishedEvent += TutorialDialogueFinished;
        GameManager.Instance.endTutorialEvent += TutorialFinished;
        GameManager.Instance.pauseStartEvent += GamePauseStart;
        GameManager.Instance.pauseEndEvent += GamePauseEnd;
        //SetUpControls();
    }

    public void SetUpControls()
    {
        controlsSet = true;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Jump.canceled += ArrestJump;
        playerInputActions.Player.Dash.performed += Dash;
        playerInputActions.Player.Crouch.performed += Crouch;

    }

    private void OnDisable()
    {
        GameManager.Instance.playerRespawnEvent -= Respawn;
        GameManager.Instance.tutorialDialogueFinishedEvent -= TutorialDialogueFinished;
        GameManager.Instance.endTutorialEvent -= TutorialFinished;
        GameManager.Instance.pauseStartEvent -= GamePauseStart;
        GameManager.Instance.pauseEndEvent -= GamePauseEnd;
        playerWalk.release();
    }

    #region Gizmos

    /*private void OnDrawGizmos()
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
    }*/
    #endregion
    
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
            if (!FmodExtensions.IsPlaying(playerWalk))
            {
                playerWalk.start();
            }
        }
        else
        {
            animator.SetBool("Running", false);
            playerWalk.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
    
    

    private void FixedUpdate()
    {
        if (isSliding)
        {
            animator.SetBool("WallSlide", true);
            rigidbody.velocity = new Vector2(rigidbody.velocity.x,
                Mathf.Clamp(rigidbody.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        //this might be inefficient
        else animator.SetBool("WallSlide", false);
        

        
        if (wallJumping)
        {
            inputAllowed = false;
            StartCoroutine(WallJumpDelayCoroutine());
            if (facingLeft)
            {
                //rigidbody.AddForce(wallJumpForce,ForceMode2D.Impulse);
                rigidbody.velocity = new Vector2(-transform.localScale.x * wallJumpForce.x, wallJumpForce.y);
            }

            if (!facingLeft)
            {
                //rigidbody.AddForce(new Vector2(-wallJumpForce.x,wallJumpForce.y),ForceMode2D.Impulse);
                rigidbody.velocity = new Vector2(-transform.localScale.x * wallJumpForce.x, wallJumpForce.y);
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

    public void GamePauseStart()
    {
        animator.SetBool("Running", false);
        playerWalk.setPaused(true);
        Time.timeScale = 0;
    }

    public void GamePauseEnd()
    {
        playerWalk.setPaused(false);
        Time.timeScale = 1;
    }

    public void Respawn()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        if (respawnCR == false)
        {
            respawnCR = true;
            isDead = true;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            yield return new WaitForSeconds(3f);
            transform.position = playerRespawnPos;
            animator.SetBool("Dead", false);
            respawnCR = false;
            isDead = false;
            yield return new WaitForSeconds(0.2f);
            animator.SetBool("Dead", false);
            rigidbody.constraints = RigidbodyConstraints2D.None;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            health.currentHealth = health.maxHealth;
            health.healthBar.SetHealth(health.maxHealth);
        }
    }
    


    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && !isDead && inputAllowed)
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

    public void TutorialDialogueFinished()
    {
        inputAllowed = true;
    }

    public void TutorialFinished()
    {
        inputAllowed = false;
    }

    void StopWallJump()
    {
        wallJumping = false;
    }

    public IEnumerator WallJumpDelayCoroutine()
    {
        inputAllowed = false;
        yield return new WaitForSeconds(controlLockDuration);
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
        if (context.performed && !isDead && inputAllowed && !isSliding)
        {
            if (!isTouchingGround && midairDash)
            {
                midairDash = false;
                StartCoroutine(DashingCoroutine());
                animator.SetBool("Dashing", true);
                if (!facingLeft)
                {
                    rigidbody.AddForce(Vector3.right * dashValue, ForceMode2D.Impulse);
                    
                    
                    //RuntimeManager.PlayOneShot("");
                }

                if (facingLeft)
                {
                    rigidbody.AddForce(Vector3.left * dashValue, ForceMode2D.Impulse);
                    
                    
                    //RuntimeManager.PlayOneShot("");
                }
            }
            else if (isTouchingGround)
            {
                StartCoroutine(DashingCoroutine());
                animator.SetBool("Dashing", true);
                if (!facingLeft)
                {
                    rigidbody.AddForce(Vector3.right * dashValue, ForceMode2D.Impulse);
                    
                    
                    //RuntimeManager.PlayOneShot("");
                }

                if (facingLeft)
                {
                    rigidbody.AddForce(Vector3.left * dashValue, ForceMode2D.Impulse);
                    
                    
                    //RuntimeManager.PlayOneShot("");
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
