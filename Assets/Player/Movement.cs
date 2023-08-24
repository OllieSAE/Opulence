using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class Movement : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    private Canvas myCanvas;
    private Combat combat;
    private PlayerInput playerInput;
    public PlayerInputActions playerInputActions;
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

    public bool dashing;
    public bool dashCooldown;
    private bool doubleJump;
    public bool midairDash;
    private bool isSliding;
    private bool wallJumping;
    private bool controlsSet;
    private bool wallJumpDelayCR;
    public bool inputAllowed;
    private bool isDead;
    private bool falconDashCooldown;
    private bool falconDashing;
    private bool noFlipping;
    
    
    [Header("Movement Values")]
    public float speed;
    public float velocity;
    public float jumpValue;
    public float dashValue;
    public float stationaryDashValue;
    public float dashCooldownDuration;
    public float wallSlidingSpeed;
    public float wallJumpDuration;
    public float controlLockDuration;
    public Vector2 wallJumpForce;
    public float falconDashWaitTime = 0.25f;
    
    public FMOD.Studio.EventInstance playerWalk;
    public FMOD.Studio.EventInstance wallSlideSFX;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        health = GetComponent<Health>();
        combat = GetComponent<Combat>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Jump.canceled += ArrestJump;
        playerInputActions.Player.Dash.performed += Dash;
        playerInputActions.Player.Pause.performed += PauseGame;
        playerInputActions.Player.Map.performed += ToggleMap;

        controlsSet = false;
        dashing = false;
        doubleJump = false;
        midairDash = false;
        wallJumping = false;
        respawnCR = false;
        isDead = false;
        wallJumpDelayCR = false;
        playerRespawnPos = transform.position;
        falconDashCooldown = false;
        falconDashing = false;
        noFlipping = false;

        playerWalk = RuntimeManager.CreateInstance("event:/SOUND EVENTS/Footsteps");
        wallSlideSFX = RuntimeManager.CreateInstance("event:/SOUND EVENTS/Wall Slide");
        RuntimeManager.AttachInstanceToGameObject(playerWalk, transform, rigidbody);
        RuntimeManager.AttachInstanceToGameObject(wallSlideSFX, transform, rigidbody);
    }

    private void PauseGame(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameManager.Instance.PauseUI();
            GameManager.Instance.currentButton = null;
        }
    }

    private void ToggleMap(InputAction.CallbackContext context)
    {
        if (context.performed) GameManager.Instance.ToggleMap();
    }

    private void OnLevelLoad()
    {

    }

    private void Start()
    {
        GameManager.Instance.onLevelLoadedEvent += OnLevelLoad;
        // if (GameManager.Instance.tutorialTestEnable)
        // {
        //     inputAllowed = false;
        // }
        //else inputAllowed = true;
        
        //is this necessary?
        inputAllowed = true;
        
        
        GameManager.Instance.playerRespawnEvent += Respawn;
        GameManager.Instance.tutorialDialogueFinishedEvent += TutorialDialogueFinished;
        GameManager.Instance.endTutorialEvent += TutorialFinished;
        GameManager.Instance.pauseStartEvent += GamePauseStart;
        GameManager.Instance.pauseEndEvent += GamePauseEnd;
        if(SceneManager.GetActiveScene().name == "FirstBossLevel")
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
        playerInputActions.Player.Jump.performed -= Jump;
        playerInputActions.Player.Jump.canceled -= ArrestJump;
        playerInputActions.Player.Dash.performed -= Dash;
        playerInputActions.Player.Map.performed -= ToggleMap;
        
        GameManager.Instance.playerRespawnEvent -= Respawn;
        GameManager.Instance.tutorialDialogueFinishedEvent -= TutorialDialogueFinished;
        GameManager.Instance.endTutorialEvent -= TutorialFinished;
        GameManager.Instance.pauseStartEvent -= GamePauseStart;
        GameManager.Instance.pauseEndEvent -= GamePauseEnd;
        playerWalk.release();
        wallSlideSFX.release();
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

        if (!isTouchingGround && isTouchingWall && rigidbody.velocity.y < 0)
        {
            isSliding = true;
        }
        else isSliding = false;

        if (isTouchingGround)
        {
            animator.SetBool("Landed", true);
        }
        else
        {
            animator.SetBool("Landed", false);
        }
        
        animator.SetFloat("Y velocity", rigidbody.velocity.y);
        
        if ((rigidbody.velocity.x > 0.01f || rigidbody.velocity.x < -0.01f) && isTouchingGround && !dashing && !falconDashing)
        {
            animator.SetBool("Running", true);
            if (!FmodExtensions.IsPlaying(playerWalk))
            {
                //playerWalk.start();
            }
        }
        else if (rigidbody.velocity.x == 0 && isTouchingGround)
        {
            animator.SetBool("Running", false);
            //playerWalk.stop(STOP_MODE.ALLOWFADEOUT);
        }
        else
        {
            animator.SetBool("Running", false);
            //playerWalk.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
    
    

    private void FixedUpdate()
    {
        if (isSliding)
        {
            animator.SetBool("WallSlide", true);
            rigidbody.velocity = new Vector2(rigidbody.velocity.x,
                Mathf.Clamp(rigidbody.velocity.y, -wallSlidingSpeed, float.MaxValue));
            if (!FmodExtensions.IsPlaying(wallSlideSFX))
            {
                wallSlideSFX.start();
            }
        }
        //this might be inefficient
        else
        {
            animator.SetBool("WallSlide", false);
            wallSlideSFX.stop(STOP_MODE.IMMEDIATE);
        }
        

        
        if (wallJumping)
        {
            inputAllowed = false;
            if (!wallJumpDelayCR)
            {
                StartCoroutine(WallJumpDelayCoroutine());
            }
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

        else if (!dashing && !isSliding && !falconDashing)
        {
            if (inputAllowed) rigidbody.velocity = new Vector2(inputVector.x * speed * Time.fixedDeltaTime, rigidbody.velocity.y);
        }


            //remove this when we have L/R sprites
        if (inputVector.x > 0 && facingLeft)
        {
            if (!noFlipping)
            {
                Flip();
            }
            
        }
        else if (inputVector.x < 0 && !facingLeft)
        {
            if (!noFlipping)
            {
                Flip();
            }
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
                GameManager.Instance.statTracker.jumps++;
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
                rigidbody.AddForce(Vector3.up * jumpValue, ForceMode2D.Impulse);
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Jump");
                doubleJump = false;
            }
            else if (isTouchingGround)
            {
                GameManager.Instance.statTracker.jumps++;
                rigidbody.AddForce(Vector3.up * jumpValue, ForceMode2D.Impulse);
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Jump");
                doubleJump = true;
            }
            else if (isSliding)
            {
                wallJumping = true;
                GameManager.Instance.statTracker.jumps++;
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Jump");
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
        wallJumpDelayCR = true;
        inputAllowed = false;
        yield return new WaitForSeconds(controlLockDuration);
        //Flip();
        inputAllowed = true;
        wallJumpDelayCR = false;
    }

    public void ArrestJump(InputAction.CallbackContext context)
    {
        if(rigidbody.velocity.y > 0) rigidbody.velocity = new Vector2(rigidbody.velocity.x,0);
    }

    //TODO: Add invulnerability to Dash, maybe with a cooldown too
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDead && inputAllowed && !isSliding && !dashCooldown)
        {
            if (!isTouchingGround && midairDash)
            {
                dashCooldown = true;
                midairDash = false;
                StartCoroutine(DashingCoroutine());
                animator.SetBool("Dashing", true);
                if (!facingLeft)
                {
                    GameManager.Instance.statTracker.dashes++;
                    rigidbody.AddForce(Vector3.right * dashValue, ForceMode2D.Impulse);
                    
                    
                    RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Dash");
                }

                if (facingLeft)
                {
                    GameManager.Instance.statTracker.dashes++;
                    rigidbody.AddForce(Vector3.left * dashValue, ForceMode2D.Impulse);
                    
                    
                    RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Dash");
                }
            }
            else if (isTouchingGround)
            {
                dashCooldown = true;
                StartCoroutine(DashingCoroutine());
                animator.SetBool("Dashing", true);
                if (!facingLeft)
                {
                    GameManager.Instance.statTracker.dashes++;
                    rigidbody.AddForce(Vector3.right * dashValue, ForceMode2D.Impulse);

                    if (rigidbody.velocity.x is < 0.01f and > -0.01f)
                    {
                        rigidbody.AddForce(Vector3.right * stationaryDashValue, ForceMode2D.Impulse);
                    }
                    RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Dash");
                }

                if (facingLeft)
                {
                    GameManager.Instance.statTracker.dashes++;
                    rigidbody.AddForce(Vector3.left * dashValue, ForceMode2D.Impulse);
                    
                    if (rigidbody.velocity.x is < 0.01f and > -0.01f)
                    {
                        rigidbody.AddForce(Vector3.left * stationaryDashValue, ForceMode2D.Impulse);
                    }
                    RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Dash");
                }
            }
        }
    }

    private IEnumerator DashingCoroutine()
    {
        dashing = true;
        health.immune = true;
        rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        yield return new WaitForSeconds(0.25f);
        dashing = false;
        health.immune = false;
        animator.SetBool("Dashing", false);
        rigidbody.constraints = RigidbodyConstraints2D.None;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        yield return new WaitForSeconds(dashCooldownDuration);
        dashCooldown = false;
    }
    
    public void FalconDash()
    {
        if (!isDead && inputAllowed && !isSliding && !falconDashCooldown)
        {
            if (!isTouchingGround && midairDash)
            {
                falconDashCooldown = true;
                midairDash = false;
                noFlipping = false;
                StartCoroutine(FalconDashCoroutine(falconDashWaitTime));
                
            }
            else if (isTouchingGround)
            {
                falconDashCooldown = true;
                noFlipping = false;
                StartCoroutine(FalconDashCoroutine(falconDashWaitTime));
                
            }
        }
    }
    

    private IEnumerator FalconDashCoroutine(float waitTime)
    {
        falconDashing = true;
        health.immune = true;
        playerInput.DeactivateInput();
        rigidbody.velocity = Vector2.zero;
        rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        yield return new WaitForSeconds(waitTime);
        combat.falconDashCollider.SetActive(true);
        noFlipping = true;
        if (!facingLeft)
        {
            rigidbody.AddForce(Vector3.right * dashValue, ForceMode2D.Impulse);
                    
                    
            RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Dash");
        }

        if (facingLeft)
        {
            rigidbody.AddForce(Vector3.left * dashValue, ForceMode2D.Impulse);
                    
                    
            RuntimeManager.PlayOneShot("event:/SOUND EVENTS/Character Dash");
        }
        yield return new WaitForSeconds(waitTime);
        falconDashing = false;
        health.immune = false;
        if(gameObject.CompareTag("Player")) animator.SetBool("FalconDash", false);
        rigidbody.constraints = RigidbodyConstraints2D.None;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        combat.falconDashCollider.SetActive(false);
        yield return new WaitForSeconds(dashCooldownDuration);
        falconDashCooldown = false;
        noFlipping = false;
        
        playerInput.ActivateInput();
        health.immune = false;
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
