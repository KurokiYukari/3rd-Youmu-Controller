using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerActionManager，用于将 InputSystem 的输入映射到 Character 的具体行为上。
/// </summary>
public class PlayerActionManager : ActionManager
{
    public void OnMove(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();

        this.characterController.Move(direction);
    }

    public void OnSpeedUp(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                this.characterController.SpeedUp();
                break;
            case InputActionPhase.Canceled:
                this.characterController.SpeedCut();
                break;
            default:
                break;
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                this.characterController.Roll();
                break;
            default:
                break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                this.characterController.Jump();
                break;
            default:
                break;
        }
    }

    public void OnSoftAttack(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                this.characterController.SoftAttack();
                break;
            default:
                break;
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                this.characterController.HeavyAttack();
                break;
            default:
                break;
        }
    }

    public void OnSightPosChange(InputAction.CallbackContext context)
    {
        this.characterController.SightPosChange(context.ReadValue<Vector2>());
    }

    public void OnSightDistance(InputAction.CallbackContext context)
    {
        this.characterController.SightDistanceChange(context.ReadValue<float>());
    }

    public void OnLock(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (this.characterController.IsLocked())
                {
                    this.characterController.UnLock();
                    return;
                }

                this.characterController.Lock();
                break;
            default:
                break;
        }
    }
}
