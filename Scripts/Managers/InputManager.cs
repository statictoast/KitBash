using System;
using System.Collections.Generic;
using UnityEngine;

[Flags] public enum InputActions
{
    None =                      1 << 0,
    JumpDown =                  1 << 1,
    JumpHeld =                  1 << 2,
    JumpReleased =              1 << 3,
    BasicAttackDown =           1 << 4,
    BasicAttackHeld =           1 << 5,
    BasicAttackReleased =       1 << 6,
    SecondaryAttackDown =       1 << 7,
    SecondaryAttackHeld =       1 << 8,
    SecondaryAttackReleased =   1 << 9,
    DodgeDown =                 1 << 10,
    DodgeHeld =                 1 << 11,
    DodgeReleased =             1 << 12,
    MagicDown =                 1 << 13,
    MagicHeld =                 1 << 14,
    MagicUp =                   1 << 15
}

public class InputFrame
{
    public Vector2 heading;
    public Vector2 aimingDirection;
    public InputActions actions;
    public float recordedTimeS;

    public InputFrame()
    {
        Clear();
    }

    public InputFrame(InputFrame aOther)
    {
        CopyFrom(aOther);
    }

    public void Clear()
    {
        actions &= 0;
        heading = Vector2.zero;
        aimingDirection = Vector2.zero;
        recordedTimeS = 0f;
    }

    public void CopyFrom(InputFrame aFrom)
    {
        heading = aFrom.heading;
        actions = aFrom.actions;
        aimingDirection = aFrom.aimingDirection;
        recordedTimeS = aFrom.recordedTimeS;
    }

    public bool HasAnyInput()
    {
        return actions > 0;
    }
}

public class InputManager : BaseManager<InputManager>
{
    private const float cDeadzoneThreshold = 0.3f;
    private const int cCommandBufferMaxSize = 60;
    private const float cMaxTimeDeltaInBufferS = 0.3f;

    // IMPORTANT: this only works if there is only 1 player playing on the machine
    private List<InputFrame> commandBuffer; // TODO: make into a ring buffer
    private InputFrame frame = new InputFrame();
    private List<InputModule> modules = new List<InputModule>();
    private Vector2 lastHeading = Vector2.right;

    public Vector2 Heading { get {  return frame.heading; } }
    public Vector2 AimingDirection { get {  return frame.aimingDirection; } }
    public Vector2 NonZeroHeading { get { return (frame.heading != Vector2.zero) ? frame.heading : lastHeading; } }

    #region Manager

    private void Awake()
    {
        modules.Add(new KeyboardInput());
        modules.Add(new ControllerInput());
        commandBuffer = new List<InputFrame>();
    }

    public override void OnRestartLevel()
    {
        commandBuffer.Clear();
        frame.Clear();
        lastHeading = Vector2.right;
    }

    private void Update()
    {
        frame.Clear();
        
        //Inputs are applied addatively
        if (modules.Count > 0)
        {
            for (int module = 0; module < modules.Count; module++)
            {
                frame.heading += modules[module].GetHeading();
                frame.aimingDirection += modules[module].GetAimingDirection();
                frame.actions ^= modules[module].GetActions();
            }

            if(Mathf.Abs(frame.heading.x) < 0.1f)
            {
                frame.heading.x = 0f;
            }

            frame.heading.Normalize();
            if (frame.heading != Vector2.zero)
            {
                lastHeading = frame.heading;
            }

            frame.aimingDirection.Normalize();
        }

        frame.recordedTimeS = Time.time;
        commandBuffer.Add(new InputFrame(frame));
        while(commandBuffer.Count > cCommandBufferMaxSize)
        {
            commandBuffer.RemoveAt(0);
        }
    }

    public bool HasAction(InputActions action, float aTimeThresholdS = 0f)
    {
        for(int i = commandBuffer.Count - 1; i >= 0; i--)
        {
            InputFrame checkFrame = commandBuffer[i];
            float deltaTime = Time.time - checkFrame.recordedTimeS;
            if(deltaTime > cMaxTimeDeltaInBufferS)
            {
                break;
            }

            if(aTimeThresholdS != 0f && checkFrame.recordedTimeS <= aTimeThresholdS)
            {
                break;
            }

            if(checkFrame.actions.HasFlag(action))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasActionThisFrame(InputActions action)
    {
        return frame.actions.HasFlag(action);
    }

    public bool IsDirectionDown(ACTION_DIRECTION aDirection, bool aUseCommandBuffer = false)
    {
        int count = commandBuffer.Count;
        switch(aDirection)
        {
            case ACTION_DIRECTION.UP:
            {
                bool currentFrameOutsideDeadzone = frame.heading.y > cDeadzoneThreshold;
                bool lookBehindOutsideDeadzone = false;
                if(aUseCommandBuffer)
                {
                    for(int i = count - 1; i >= 0; i--)
                    {
                        float deltaTime = Time.time - commandBuffer[i].recordedTimeS;
                        if(deltaTime > cMaxTimeDeltaInBufferS)
                        {
                            break;
                        }
                        lookBehindOutsideDeadzone |= commandBuffer[i].heading.y > cDeadzoneThreshold;
                    }
                }
                return currentFrameOutsideDeadzone || lookBehindOutsideDeadzone;
            }
            case ACTION_DIRECTION.DOWN:
            {
                bool currentFrameOutsideDeadzone = frame.heading.y < -cDeadzoneThreshold;
                bool lookBehindOutsideDeadzone = false;
                if(aUseCommandBuffer)
                {
                    for(int i = count - 1; i >= 0; i--)
                    {
                        float deltaTime = Time.time - commandBuffer[i].recordedTimeS;
                        if(deltaTime > cMaxTimeDeltaInBufferS)
                        {
                            break;
                        }
                        lookBehindOutsideDeadzone |= commandBuffer[i].heading.y < -cDeadzoneThreshold;
                    }
                }
                return currentFrameOutsideDeadzone || lookBehindOutsideDeadzone;
            }
            case ACTION_DIRECTION.SIDE:
            {
                bool currentFrameOutsideDeadzone = frame.heading.x > cDeadzoneThreshold || frame.heading.x < -cDeadzoneThreshold;
                bool lookBehindOutsideDeadzone = false;
                if(aUseCommandBuffer)
                {
                    for(int i = count - 1; i >= 0; i--)
                    {
                        float deltaTime = Time.time - commandBuffer[i].recordedTimeS;
                        if(deltaTime > cMaxTimeDeltaInBufferS)
                        {
                            break;
                        }
                        lookBehindOutsideDeadzone |= commandBuffer[i].heading.x > cDeadzoneThreshold || commandBuffer[i].heading.x < -cDeadzoneThreshold;
                    }
                }
                return currentFrameOutsideDeadzone || lookBehindOutsideDeadzone;
            }
            default:
            {
                return true;
            }
        }
    }
    #endregion

    public void DebugPrintCommandBuffer()
    {
        for(int i = commandBuffer.Count - 1; i >= 0; i--)
        {
            InputFrame checkFrame = commandBuffer[i];
            float timeDelta = Time.time - checkFrame.recordedTimeS;
            bool isValid = timeDelta <= cMaxTimeDeltaInBufferS;
            string debugString = string.Format("Frame: {0}   timeSinceNow: {1}     IsValid: {2}", i, timeDelta, isValid);
            Debug.Log(debugString);
        }
    }
}