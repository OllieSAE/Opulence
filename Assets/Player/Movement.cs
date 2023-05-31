using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{

    private Rigidbody rigidbody;
    private PlayerInput playerInput;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        PlayerInputActions playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Movement.performed += Move;
        playerInputActions.Player.Dash.performed += Dash;
        playerInputActions.Player.Crouch.performed += Crouch;
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if(context.performed) ;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if(context.performed) ;
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
