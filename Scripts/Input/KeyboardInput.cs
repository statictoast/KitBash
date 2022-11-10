using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInput : InputModule
{
    private Keyboard m_keyboard;
    private Mouse m_mouse;

    public KeyboardInput()
    {
        m_keyboard = InputSystem.GetDevice<Keyboard>();
        m_mouse = InputSystem.GetDevice<Mouse>();
    }

    // TODO: have an input mapping manager to allow for button remapping
    public override Vector2 GetHeading()
    {
        Vector2 newHeading = Vector2.zero;

        if (m_keyboard.aKey.isPressed)
        {
            newHeading.x = -1f;
        }
        else if (m_keyboard.dKey.isPressed)
        {
            newHeading.x = 1f;
        }

        if (m_keyboard.sKey.isPressed)
        {
            newHeading.y = -1f;
        }
        else if (m_keyboard.wKey.isPressed)
        {
            newHeading.y = 1f;
        }

        return newHeading;
    }

    public override Vector2 GetAimingDirection()
    {
        Vector2 newUnitDirection = Vector2.zero;

        /*Unit localPlayer = UnitManager.Instance.GetLocalPlayer();
        if(localPlayer == null)
        {
            return newUnitDirection;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = localPlayer.transform.position.z;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Vector3 positionDelta = mousePosition - localPlayer.transform.position;
        newUnitDirection.x = positionDelta.x;
        newUnitDirection.y = positionDelta.y;*/

        return newUnitDirection;
    }

    public override InputActions GetActions()
    {
        InputActions newActions = InputActions.None;

        // Jump
        if (m_keyboard.spaceKey.wasPressedThisFrame)
        {
            newActions |= InputActions.JumpDown;
        }

        if (m_keyboard.spaceKey.isPressed)
        {
            newActions |= InputActions.JumpHeld;
        }

        if (m_keyboard.spaceKey.wasReleasedThisFrame)
        {
            newActions |= InputActions.JumpReleased;
        }

        if(IsMouseInSideViewport())
        {
            // Basic Attack
            if(m_mouse.leftButton.wasPressedThisFrame)
            {
                newActions |= InputActions.BasicAttackDown;
            }

            if(m_mouse.leftButton.isPressed)
            {
                newActions |= InputActions.BasicAttackHeld;
            }

            if(m_mouse.leftButton.wasReleasedThisFrame)
            {
                newActions |= InputActions.BasicAttackReleased;
            }

            // Secondary Attack
            if(m_mouse.rightButton.wasPressedThisFrame)
            {
                newActions |= InputActions.SecondaryAttackDown;
            }

            if(m_mouse.rightButton.isPressed)
            {
                newActions |= InputActions.SecondaryAttackHeld;
            }

            if(m_mouse.rightButton.wasReleasedThisFrame)
            {
                newActions |= InputActions.SecondaryAttackReleased;
            }
        }

        // Dodge
        if(m_keyboard.leftShiftKey.wasPressedThisFrame)
        {
            newActions |= InputActions.DodgeDown;
        }

        if(m_keyboard.leftShiftKey.isPressed)
        {
            newActions |= InputActions.DodgeHeld;
        }

        if(m_keyboard.leftShiftKey.wasReleasedThisFrame)
        {
            newActions |= InputActions.DodgeReleased;
        }

        // Magic
        if(m_keyboard.eKey.wasPressedThisFrame)
        {
            newActions |= InputActions.MagicDown;
        }

        if(m_keyboard.eKey.isPressed)
        {
            newActions |= InputActions.MagicHeld;
        }

        if(m_keyboard.eKey.wasReleasedThisFrame)
        {
            newActions |= InputActions.MagicUp;
        }

        return newActions;
    }

    public bool IsMouseInSideViewport()
    {
        // TODO: figure out how to get viewport dimensions to get max bounds
        Vector2 mousePos = m_mouse.position.ReadValue();
        return mousePos.x >= 0f && mousePos.y >= 0f && mousePos.x <= Screen.width && mousePos.y <= Screen.height;
    }
}
