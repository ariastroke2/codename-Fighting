using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerActions pa;
    private Dpad dp;

    void Start()
    {
        pa = GetComponent<PlayerActions>();
        dp = new Dpad();
        pa.AssignDpad(ref dp);
    }

    public void DirectionalInput(InputAction.CallbackContext ctx)
    {
        Direction button = Enum.Parse<Direction>(ctx.action.name);
        if(ctx.started)
            dp.Press(button);
        if(ctx.canceled) 
            dp.Release(button);

    }

    public void ExecuteAttack(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
            pa.ExecuteInput(ctx.action.name);
        if (ctx.canceled)
            pa.ReleaseInput(ctx.action.name);
    }
}
