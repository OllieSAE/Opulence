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

    public float speed;
    public float jumpValue;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Movement.performed += Move;
        playerInputActions.Player.Dash.performed += Dash;
        playerInputActions.Player.Crouch.performed += Crouch;
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        rigidbody.AddForce(new Vector3(inputVector.x,0,inputVector.y) * speed, ForceMode2D.Force);

        //maybe cap speed at 5 and apply 1000 force?
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

    public void Dash(InputAction.CallbackContext context)
    {
        if(context.performed) ;
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if(context.performed) ;
    }
}
