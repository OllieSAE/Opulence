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

    //delete once L/R sprites exist
    private bool facingLeft;
    
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
        playerInputActions.Player.Movement.performed += Move;
        playerInputActions.Player.Dash.performed += Dash;
        playerInputActions.Player.Crouch.performed += Crouch;
    }

    private void Update()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        //this is preventing Dash from working
        rigidbody.velocity = new Vector2(inputVector.x * speed * Time.fixedDeltaTime, rigidbody.velocity.y);
        
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
            rigidbody.AddForce(Vector3.up * jumpValue, ForceMode2D.Impulse);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        
    }

    public void ArrestJump(InputAction.CallbackContext context)
    {
        if(rigidbody.velocity.y > 0) rigidbody.velocity = new Vector2(rigidbody.velocity.x,0);
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!facingLeft)
            {
                rigidbody.AddForce(Vector3.right * dashValue, ForceMode2D.Impulse);
                print("dash right" + rigidbody.velocity.x);
            }

            if (facingLeft)
            {
                rigidbody.AddForce(Vector3.left * dashValue, ForceMode2D.Impulse);
                print("dash left" + rigidbody.velocity.x);
            }
        }
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
