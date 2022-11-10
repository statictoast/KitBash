using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInput : InputModule
{
    private Gamepad m_gamepad;

    public ControllerInput ()
    {
        m_gamepad = Gamepad.current;
    }

    public override Vector2 GetHeading()
    {
        if(m_gamepad == null)
        {
            return Vector2.zero;
        }

        return new Vector2(m_gamepad.leftStick.x.ReadValue(), -m_gamepad.leftStick.y.ReadValue());
    }

    public override Vector2 GetAimingDirection()
    {
        if(m_gamepad == null)
        {
            return Vector2.zero;
        }

        return new Vector2(m_gamepad.rightStick.x.ReadValue(), -m_gamepad.rightStick.y.ReadValue());
    }

    public override InputActions GetActions()
    {
        InputActions newActions = InputActions.None;
        if(m_gamepad == null)
        {
            m_gamepad = Gamepad.current;
            return newActions;
        }

        // Jumping
        if (m_gamepad.buttonSouth.wasPressedThisFrame) // A
        {
            newActions |= InputActions.JumpDown;
        }

        if (m_gamepad.buttonSouth.isPressed)
        {
            newActions |= InputActions.JumpHeld;
        }

        if (m_gamepad.buttonSouth.wasReleasedThisFrame)
        {
            newActions |= InputActions.JumpReleased;
        }

        // Basic Attack
        if (m_gamepad.buttonWest.wasPressedThisFrame) // X
        {
            newActions |= InputActions.BasicAttackDown;
        }

        if (m_gamepad.buttonWest.isPressed)
        {
            newActions |= InputActions.BasicAttackHeld;
        }

        if (m_gamepad.buttonWest.wasReleasedThisFrame)
        {
            newActions |= InputActions.BasicAttackReleased;
        }

        // Secondary Attack
        if (m_gamepad.buttonEast.wasPressedThisFrame) // B
        {
            newActions |= InputActions.SecondaryAttackDown;
        }

        if (m_gamepad.buttonEast.isPressed)
        {
            newActions |= InputActions.SecondaryAttackHeld;
        }

        if (m_gamepad.buttonEast.wasReleasedThisFrame)
        {
            newActions |= InputActions.SecondaryAttackReleased;
        }

        // Dodge
        if(m_gamepad.rightShoulder.wasPressedThisFrame) // RB
        {
            newActions |= InputActions.DodgeDown;
        }

        if(m_gamepad.rightShoulder.isPressed)
        {
            newActions |= InputActions.DodgeHeld;
        }

        if(m_gamepad.rightShoulder.wasReleasedThisFrame)
        {
            newActions |= InputActions.DodgeReleased;
        }

        // Magic
        if(m_gamepad.rightTrigger.wasPressedThisFrame)
        {
            newActions |= InputActions.MagicDown;
        }

        if(m_gamepad.rightTrigger.isPressed)
        {
            newActions |= InputActions.MagicHeld;
        }

        if(m_gamepad.rightTrigger.wasReleasedThisFrame)
        {
            newActions |= InputActions.MagicUp;
        }

        return newActions;
    }
}
